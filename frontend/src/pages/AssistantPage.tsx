import { useCallback, useEffect, useLayoutEffect, useMemo, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  ArrowLeft,
  ArrowUp,
  BarChart3,
  Boxes,
  Check,
  ChevronDown,
  Clock,
  Cpu,
  Mic,
  PackagePlus,
  PanelLeft,
  Plus,
  Settings,
  Sparkles,
  SquarePen,
  Trash2,
} from 'lucide-react'
import { useAuthStore } from '@/stores/auth-store'
import { useDashboard } from '@/features/sampleTracking/api'
import { getAssistantReply } from '@/components/assistant/engine'
import styles from './AssistantPage.module.css'

/* ------------------------------------------------------------------ */
/*  Types & persistence                                                */
/* ------------------------------------------------------------------ */

interface ChatMessage {
  id: string
  role: 'user' | 'assistant'
  text: string
  to?: string
  toLabel?: string
}

interface Conversation {
  id: string
  title: string
  messages: ChatMessage[]
  updatedAt: number
}

type Phase = 'idle' | 'thinking' | 'streaming'

const STORAGE_KEY = 'sr_assistant_v1'
const MODELS = ['Rack AI · Swift', 'Rack AI · Pro', 'Rack AI · Reasoning'] as const
const PLACEHOLDERS = [
  'Ask anything about your samples…',
  'How do I register a new sample?',
  'What’s overdue right now?',
  'Show me my live stats',
  'Where can I find the 3D rack?',
]

const uid = () => Math.random().toString(36).slice(2, 10) + Date.now().toString(36).slice(-4)

function newConversation(): Conversation {
  return { id: uid(), title: 'New chat', messages: [], updatedAt: Date.now() }
}

function loadConversations(): Conversation[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (raw) {
      const parsed = JSON.parse(raw) as Conversation[]
      if (Array.isArray(parsed) && parsed.length) return parsed
    }
  } catch {
    /* ignore */
  }
  return [newConversation()]
}

/* ------------------------------------------------------------------ */
/*  Main page                                                          */
/* ------------------------------------------------------------------ */

export default function AssistantPage() {
  const navigate = useNavigate()
  const user = useAuthStore((s) => s.user)
  const { data: dash } = useDashboard()
  const firstName = (user?.fullName?.trim() || user?.username || 'there').split(' ')[0]

  const [conversations, setConversations] = useState<Conversation[]>(loadConversations)
  const [activeId, setActiveId] = useState<string>(() => conversations[0]?.id ?? '')
  const [input, setInput] = useState('')
  const [phase, setPhase] = useState<Phase>('idle')
  const [sidebarOpen, setSidebarOpen] = useState(true)
  const [mobileNav, setMobileNav] = useState(false)
  const [model, setModel] = useState<string>(MODELS[1])
  const [modelOpen, setModelOpen] = useState(false)
  const [settingsOpen, setSettingsOpen] = useState(false)
  const [listening, setListening] = useState(false)
  const [placeholderIdx, setPlaceholderIdx] = useState(0)

  const textareaRef = useRef<HTMLTextAreaElement>(null)
  const scrollRef = useRef<HTMLDivElement>(null)
  const endRef = useRef<HTMLDivElement>(null)
  const thinkTimer = useRef<number | null>(null)
  const streamTimer = useRef<number | null>(null)
  const recognitionRef = useRef<{ stop: () => void } | null>(null)

  const active = conversations.find((c) => c.id === activeId) ?? conversations[0]
  const messages = active?.messages ?? []
  const isEmpty = messages.length === 0 && phase === 'idle'

  /* ---- persistence ---- */
  useEffect(() => {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(conversations))
    } catch {
      /* ignore */
    }
  }, [conversations])

  /* ---- rotating placeholder (only when input empty) ---- */
  useEffect(() => {
    if (input) return
    const t = window.setInterval(() => setPlaceholderIdx((i) => (i + 1) % PLACEHOLDERS.length), 3800)
    return () => window.clearInterval(t)
  }, [input])

  /* ---- auto-scroll ---- */
  useEffect(() => {
    endRef.current?.scrollIntoView({ behavior: 'smooth', block: 'end' })
  }, [messages, phase])

  /* ---- cleanup timers ---- */
  useEffect(
    () => () => {
      if (thinkTimer.current) window.clearTimeout(thinkTimer.current)
      if (streamTimer.current) window.clearInterval(streamTimer.current)
    },
    [],
  )

  /* ---- textarea auto-resize ---- */
  useLayoutEffect(() => {
    const el = textareaRef.current
    if (!el) return
    el.style.height = 'auto'
    el.style.height = Math.min(el.scrollHeight, 220) + 'px'
  }, [input])

  /* ---- conversation mutators ---- */
  const patchConversation = useCallback((id: string, fn: (c: Conversation) => Conversation) => {
    setConversations((prev) => prev.map((c) => (c.id === id ? fn(c) : c)))
  }, [])

  const appendMessage = useCallback(
    (id: string, msg: ChatMessage) => {
      patchConversation(id, (c) => {
        const title = c.messages.length === 0 && msg.role === 'user' ? msg.text.slice(0, 44) : c.title
        return { ...c, title, messages: [...c.messages, msg], updatedAt: Date.now() }
      })
    },
    [patchConversation],
  )

  const updateMessage = useCallback(
    (convId: string, msgId: string, patch: Partial<ChatMessage>) => {
      patchConversation(convId, (c) => ({
        ...c,
        messages: c.messages.map((m) => (m.id === msgId ? { ...m, ...patch } : m)),
      }))
    },
    [patchConversation],
  )

  /* ---- send ---- */
  const send = useCallback(
    (raw: string) => {
      const text = raw.trim()
      if (!text || phase !== 'idle' || !active) return
      const convId = active.id

      appendMessage(convId, { id: uid(), role: 'user', text })
      setInput('')
      setPhase('thinking')

      const reply = getAssistantReply(text, { kpis: dash?.kpis, firstName })

      thinkTimer.current = window.setTimeout(() => {
        const msgId = uid()
        appendMessage(convId, { id: msgId, role: 'assistant', text: '' })
        setPhase('streaming')

        const full = reply.text
        const step = Math.max(1, Math.ceil(full.length / 46))
        let i = 0
        streamTimer.current = window.setInterval(() => {
          i += step
          updateMessage(convId, msgId, { text: full.slice(0, i) })
          if (i >= full.length) {
            if (streamTimer.current) window.clearInterval(streamTimer.current)
            streamTimer.current = null
            if (reply.to) updateMessage(convId, msgId, { to: reply.to, toLabel: reply.toLabel })
            setPhase('idle')
          }
        }, 22)
      }, 460)
    },
    [phase, active, appendMessage, updateMessage, dash?.kpis, firstName],
  )

  /* ---- conversation list actions ---- */
  function startNewChat() {
    const current = conversations.find((c) => c.id === activeId)
    if (current && current.messages.length === 0) {
      textareaRef.current?.focus()
      setMobileNav(false)
      return
    }
    const conv = newConversation()
    setConversations((prev) => [conv, ...prev])
    setActiveId(conv.id)
    setMobileNav(false)
    setTimeout(() => textareaRef.current?.focus(), 30)
  }

  function selectChat(id: string) {
    setActiveId(id)
    setMobileNav(false)
  }

  function deleteChat(id: string) {
    setConversations((prev) => {
      const next = prev.filter((c) => c.id !== id)
      const list = next.length ? next : [newConversation()]
      if (id === activeId) setActiveId(list[0].id)
      return list
    })
  }

  function clearAll() {
    const conv = newConversation()
    setConversations([conv])
    setActiveId(conv.id)
    setSettingsOpen(false)
  }

  /* ---- voice input (Web Speech API, progressive enhancement) ---- */
  function toggleVoice() {
    if (listening) {
      recognitionRef.current?.stop()
      return
    }
    const w = window as unknown as { SpeechRecognition?: new () => any; webkitSpeechRecognition?: new () => any }
    const SR = w.SpeechRecognition || w.webkitSpeechRecognition
    if (!SR) {
      setInput((v) => v) // no-op; feature unsupported
      return
    }
    const rec = new SR()
    rec.lang = 'en-US'
    rec.interimResults = true
    rec.continuous = false
    let finalText = ''
    rec.onresult = (e: any) => {
      let interim = ''
      for (let i = e.resultIndex; i < e.results.length; i++) {
        const t = e.results[i][0].transcript
        if (e.results[i].isFinal) finalText += t
        else interim += t
      }
      setInput((finalText + interim).trim())
    }
    rec.onend = () => {
      setListening(false)
      recognitionRef.current = null
    }
    rec.onerror = () => {
      setListening(false)
      recognitionRef.current = null
    }
    recognitionRef.current = rec
    setListening(true)
    rec.start()
  }

  const voiceSupported = useMemo(() => {
    const w = window as unknown as Record<string, unknown>
    return typeof window !== 'undefined' && ('SpeechRecognition' in w || 'webkitSpeechRecognition' in w)
  }, [])

  function onKeyDown(e: React.KeyboardEvent<HTMLTextAreaElement>) {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      send(input)
    }
  }

  const suggestions = [
    { icon: <BarChart3 size={18} />, title: 'Show my live stats', sub: 'Totals, overdue, testing & free slots', prompt: 'Show my live stats' },
    { icon: <PackagePlus size={18} />, title: 'Register a sample', sub: 'Walk me through receiving', prompt: 'How do I register a sample?' },
    { icon: <Boxes size={18} />, title: 'Open the 3D rack', sub: 'Explore both racks in 3D', prompt: 'Open the 3D rack' },
    { icon: <Clock size={18} />, title: 'What’s overdue?', sub: 'Samples aged 90+ days', prompt: 'What samples are overdue?' },
  ]

  return (
    <div className={styles.app} data-sidebar={sidebarOpen ? 'open' : 'closed'}>
      {/* ambient background */}
      <div className={styles.ambient} aria-hidden>
        <span className={styles.glowA} />
        <span className={styles.glowB} />
        <span className={styles.grid} />
      </div>

      {/* ---------------- Sidebar ---------------- */}
      <aside className={`${styles.sidebar} ${mobileNav ? styles.sidebarMobileOpen : ''}`}>
        <div className={styles.sideHead}>
          <div className={styles.brand}>
            <span className={styles.brandMark}>
              <Sparkles size={16} />
            </span>
            {sidebarOpen && (
              <div className={styles.brandText}>
                <strong>Rack&nbsp;AI</strong>
                <span>Assistant</span>
              </div>
            )}
          </div>
        </div>

        <button className={styles.newChat} onClick={startNewChat}>
          <SquarePen size={16} />
          {sidebarOpen && <span>New chat</span>}
        </button>

        {sidebarOpen && (
          <div className={styles.history}>
            <p className={styles.histLabel}>Recent</p>
            <div className={styles.histList}>
              {conversations.map((c) => (
                <div key={c.id} className={`${styles.histItem} ${c.id === activeId ? styles.histItemActive : ''}`}>
                  <button className={styles.histBtn} onClick={() => selectChat(c.id)} title={c.title}>
                    <span className={styles.histDot} />
                    <span className={styles.histTitle}>{c.title || 'New chat'}</span>
                  </button>
                  <button className={styles.histDelete} onClick={() => deleteChat(c.id)} aria-label="Delete conversation">
                    <Trash2 size={14} />
                  </button>
                </div>
              ))}
            </div>
          </div>
        )}

        <div className={styles.sideFoot}>
          <button className={styles.backBtn} onClick={() => navigate('/sample-tracking/dashboard')}>
            <ArrowLeft size={16} />
            {sidebarOpen && <span>Back to app</span>}
          </button>
        </div>
      </aside>

      {mobileNav && <div className={styles.scrim} onClick={() => setMobileNav(false)} />}

      {/* ---------------- Main ---------------- */}
      <main className={styles.main}>
        <header className={styles.topbar}>
          <div className={styles.topLeft}>
            <button className={styles.iconBtn} onClick={() => setSidebarOpen((v) => !v)} aria-label="Toggle sidebar">
              <PanelLeft size={18} />
            </button>
            <button className={`${styles.iconBtn} ${styles.mobileOnly}`} onClick={() => setMobileNav(true)} aria-label="Open menu">
              <Boxes size={18} />
            </button>

            <div className={styles.modelWrap}>
              <button className={styles.modelBtn} onClick={() => setModelOpen((v) => !v)}>
                <Cpu size={15} />
                <span>{model}</span>
                <ChevronDown size={14} className={modelOpen ? styles.flip : ''} />
              </button>
              {modelOpen && (
                <div className={styles.menu} role="menu">
                  {MODELS.map((m) => (
                    <button
                      key={m}
                      className={styles.menuItem}
                      onClick={() => {
                        setModel(m)
                        setModelOpen(false)
                      }}
                    >
                      <span>{m}</span>
                      {m === model && <Check size={15} />}
                    </button>
                  ))}
                </div>
              )}
            </div>
          </div>

          <div className={styles.topRight}>
            <span className={styles.statusPill}>
              <span className={styles.statusDot} /> Online
            </span>
            <div className={styles.modelWrap}>
              <button className={styles.iconBtn} onClick={() => setSettingsOpen((v) => !v)} aria-label="Settings">
                <Settings size={18} />
              </button>
              {settingsOpen && (
                <div className={`${styles.menu} ${styles.menuRight}`} role="menu">
                  <button className={styles.menuItem} onClick={() => navigate('/sample-tracking/settings')}>
                    App settings
                  </button>
                  <button className={styles.menuItem} onClick={clearAll}>
                    Clear conversations
                  </button>
                  <button className={styles.menuItem} onClick={() => navigate('/sample-tracking/dashboard')}>
                    Back to dashboard
                  </button>
                </div>
              )}
            </div>
          </div>
        </header>

        {/* ----- thread / empty ----- */}
        <div className={styles.scroll} ref={scrollRef} onClick={() => { setModelOpen(false); setSettingsOpen(false) }}>
          {isEmpty ? (
            <div className={styles.empty}>
              <div className={styles.orb} aria-hidden>
                <span className={styles.orbCore} />
                <span className={styles.orbRing} />
                <span className={styles.orbRing2} />
              </div>
              <h1 className={styles.emptyTitle}>
                What can I help you explore, <span className={styles.gradText}>{firstName}</span>?
              </h1>
              <p className={styles.emptySub}>
                Your sample-racking copilot — ask about inventory, testing, aging, the 3D rack, or your live numbers.
              </p>
              <div className={styles.cards}>
                {suggestions.map((s) => (
                  <button key={s.title} className={styles.card} onClick={() => send(s.prompt)}>
                    <span className={styles.cardIcon}>{s.icon}</span>
                    <span className={styles.cardBody}>
                      <strong>{s.title}</strong>
                      <span>{s.sub}</span>
                    </span>
                  </button>
                ))}
              </div>
            </div>
          ) : (
            <div className={styles.thread}>
              {messages.map((m) =>
                m.role === 'user' ? (
                  <div key={m.id} className={styles.rowUser}>
                    <div className={styles.userBubble}>{m.text}</div>
                  </div>
                ) : (
                  <div key={m.id} className={styles.rowBot}>
                    <span className={styles.botAvatar} aria-hidden>
                      <Sparkles size={15} />
                    </span>
                    <div className={styles.botBubble}>
                      <div className={styles.botText}>
                        {m.text}
                        {phase === 'streaming' && m.text && m.id === messages[messages.length - 1].id && (
                          <span className={styles.caret} />
                        )}
                      </div>
                      {m.to && (
                        <button className={styles.deepLink} onClick={() => navigate(m.to!)}>
                          {m.toLabel ?? 'Open'} <ArrowUp size={14} className={styles.deepLinkIcon} />
                        </button>
                      )}
                    </div>
                  </div>
                ),
              )}

              {phase === 'thinking' && (
                <div className={styles.rowBot}>
                  <span className={styles.botAvatar} aria-hidden>
                    <Sparkles size={15} />
                  </span>
                  <div className={styles.botBubble}>
                    <span className={styles.typing}>
                      <i />
                      <i />
                      <i />
                    </span>
                  </div>
                </div>
              )}
              <div ref={endRef} />
            </div>
          )}
        </div>

        {/* ----- composer ----- */}
        <div className={styles.composerWrap}>
          <div className={`${styles.composer} ${listening ? styles.composerLive : ''}`}>
            <div className={styles.composerGlow} aria-hidden />
            <div className={styles.composerInner}>
              <button className={styles.circleBtn} title="Attach (coming soon)" aria-label="Attach">
                <Plus size={18} />
              </button>

              <textarea
                ref={textareaRef}
                className={styles.textarea}
                value={input}
                onChange={(e) => setInput(e.target.value)}
                onKeyDown={onKeyDown}
                rows={1}
                placeholder={listening ? 'Listening…' : PLACEHOLDERS[placeholderIdx]}
                aria-label="Message"
              />

              <div className={styles.composerActions}>
                <button
                  className={`${styles.circleBtn} ${listening ? styles.micLive : ''}`}
                  onClick={toggleVoice}
                  title={voiceSupported ? 'Voice input' : 'Voice input not supported in this browser'}
                  aria-label="Voice input"
                  disabled={!voiceSupported}
                >
                  <Mic size={18} />
                </button>
                <button
                  className={styles.send}
                  onClick={() => send(input)}
                  disabled={!input.trim() || phase !== 'idle'}
                  aria-label="Send message"
                >
                  <ArrowUp size={18} />
                </button>
              </div>
            </div>
          </div>
          <p className={styles.hint}>
            <kbd>⏎</kbd> send · <kbd>⇧</kbd>&nbsp;<kbd>⏎</kbd> new line · Rack AI can make mistakes — verify important actions.
          </p>
        </div>
      </main>
    </div>
  )
}
