import { useEffect, useMemo, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Bot, Send, Sparkles, X } from 'lucide-react'
import { useAuthStore } from '@/stores/auth-store'
import { useDashboard } from '@/features/sampleTracking/api'
import { getAssistantReply } from './engine'
import styles from './ChatAssistant.module.css'

interface Msg {
  role: 'bot' | 'user'
  text: string
  to?: string
  toLabel?: string
}

const GREET_KEY = 'sr_greet'

export function ChatAssistant() {
  const navigate = useNavigate()
  const user = useAuthStore((s) => s.user)
  const { data: dash } = useDashboard()
  const [open, setOpen] = useState(false)
  const [messages, setMessages] = useState<Msg[]>([])
  const [input, setInput] = useState('')
  const [typing, setTyping] = useState(false)
  const endRef = useRef<HTMLDivElement>(null)

  const name = user?.fullName?.trim() || user?.username || 'there'
  const firstName = name.split(' ')[0]

  // Pop up and greet right after login.
  useEffect(() => {
    let greet = false
    try {
      greet = sessionStorage.getItem(GREET_KEY) === '1'
    } catch {
      /* ignore */
    }
    if (greet && user) {
      try {
        sessionStorage.removeItem(GREET_KEY)
      } catch {
        /* ignore */
      }
      setOpen(true)
      setMessages([
        {
          role: 'bot',
          text: `Welcome back, ${firstName}! 👋 I'm your Sample Racking assistant. Ask me how to register a sample, check aging, run reports — or just type "help".`,
        },
      ])
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [user])

  useEffect(() => {
    endRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages, open, typing])

  const quickReplies = useMemo(
    () => ['Show my stats', 'Register a sample', 'View 3D rack', 'What can you do?'],
    [],
  )

  function openChat() {
    setOpen(true)
    if (messages.length === 0) {
      setMessages([{ role: 'bot', text: `Hi ${firstName}! How can I help with your samples today?` }])
    }
  }

  function send(text: string) {
    const q = text.trim()
    if (!q) return
    setMessages((prev) => [...prev, { role: 'user', text: q }])
    setInput('')
    setTyping(true)
    window.setTimeout(() => {
      const reply = getAssistantReply(q, { kpis: dash?.kpis, firstName })
      setTyping(false)
      setMessages((prev) => [...prev, { role: 'bot', ...reply }])
    }, 420)
  }

  if (!user) return null

  return (
    <>
      {!open && (
        <button className={styles.fab} onClick={openChat} aria-label="Open assistant">
          <Bot size={24} />
          <span className={styles.fabDot} />
        </button>
      )}

      {open && (
        <div className={styles.panel} role="dialog" aria-label="Rack assistant">
          <div className={styles.header}>
            <span className={styles.headerTitle}>
              <Sparkles size={16} /> Rack Assistant
            </span>
            <button className={styles.close} onClick={() => setOpen(false)} aria-label="Close">
              <X size={18} />
            </button>
          </div>

          <div className={styles.body}>
            {messages.map((m, i) => (
              <div key={i} className={m.role === 'bot' ? styles.botRow : styles.userRow}>
                <div className={m.role === 'bot' ? styles.botBubble : styles.userBubble}>
                  <span>{m.text}</span>
                  {m.to && (
                    <button
                      className={styles.actionBtn}
                      onClick={() => {
                        navigate(m.to!)
                        setOpen(false)
                      }}
                    >
                      {m.toLabel ?? 'Open'}
                    </button>
                  )}
                </div>
              </div>
            ))}
            {typing && (
              <div className={styles.botRow}>
                <div className={styles.botBubble}>
                  <span className={styles.dots}>
                    <i />
                    <i />
                    <i />
                  </span>
                </div>
              </div>
            )}
            <div ref={endRef} />
          </div>

          <div className={styles.quick}>
            {quickReplies.map((qr) => (
              <button key={qr} className={styles.chip} onClick={() => send(qr)}>
                {qr}
              </button>
            ))}
          </div>

          <form
            className={styles.inputRow}
            onSubmit={(e) => {
              e.preventDefault()
              send(input)
            }}
          >
            <input
              className={styles.input}
              value={input}
              onChange={(e) => setInput(e.target.value)}
              placeholder="Ask me anything…"
              autoFocus
            />
            <button className={styles.sendBtn} type="submit" aria-label="Send">
              <Send size={16} />
            </button>
          </form>
        </div>
      )}
    </>
  )
}

