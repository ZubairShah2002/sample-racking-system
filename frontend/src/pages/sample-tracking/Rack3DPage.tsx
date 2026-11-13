import { Suspense, useEffect, useMemo, useRef, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { Canvas, useThree } from '@react-three/fiber'
import { CameraControls, Edges, Html } from '@react-three/drei'
import type CameraControlsImpl from 'camera-controls'
import * as THREE from 'three'
import { Grid3x3, RotateCcw, Undo2 } from 'lucide-react'
import { useRackLocationDetail, useRackOverview } from '@/features/sampleTracking/api'
import { useSampleTrackingAccess } from '@/features/sampleTracking/useAccess'
import { PageHeader, SampleStatusBadge, AgingBadge } from '@/features/sampleTracking/components/common'
import { Button } from '@/components/ui/Button'
import { OCCUPANCY_COLORS, OCCUPANCY_LABELS, OCCUPANCY_LEGEND, formatDate } from '@/features/sampleTracking/constants'
import type { RackOccupancy } from '@/features/sampleTracking/types'
import shared from '@/features/sampleTracking/components/shared.module.css'
import styles from './Rack3D.module.css'

const SLOT_W = 1.1
const SLOT_H = 0.8
const SLOT_D = 1.1
const GAP_X = 0.14
const GAP_Y = 0.2
const BASE_Y = 0.15
const POST = 0.09
const HOME_POS: [number, number, number] = [4.8, 3.6, 6.4]

interface Placed extends RackOccupancy {
  x: number
  y: number
  z: number
}

function layout(locations: RackOccupancy[], maxPosition: number, zones: string[]) {
  const zoneIndex = new Map(zones.map((z, i) => [z, i]))
  const zoneSpan = SLOT_D + 1.6
  return locations.map((l) => ({
    ...l,
    x: (l.position - (maxPosition + 1) / 2) * (SLOT_W + GAP_X),
    y: (l.level - 1) * (SLOT_H + GAP_Y) + SLOT_H / 2 + BASE_Y,
    z: (zoneIndex.get(l.zone) ?? 0) * zoneSpan,
  })) as Placed[]
}

// ---- MYSTAR rack textures (procedural, no network) ----
function makeUprightTexture() {
  const c = document.createElement('canvas')
  c.width = 64; c.height = 220
  const g = c.getContext('2d')!
  // Brighter painted-steel blue, matching the real MYSTAR uprights.
  const grd = g.createLinearGradient(0, 0, 64, 0)
  grd.addColorStop(0, '#3f6fd6'); grd.addColorStop(0.5, '#2f5cc4'); grd.addColorStop(1, '#2149a6')
  g.fillStyle = grd; g.fillRect(0, 0, 64, 220)
  // Channel edge highlights down both sides of the post face.
  g.fillStyle = 'rgba(255,255,255,0.12)'; g.fillRect(2, 0, 3, 220); g.fillRect(59, 0, 3, 220)
  g.fillStyle = 'rgba(0,0,0,0.10)'; g.fillRect(0, 0, 2, 220); g.fillRect(62, 0, 2, 220)
  // Two vertical columns of densely packed keyhole slots, like the real upright.
  for (let y = 11; y < 220; y += 18) for (const x of [21, 43]) {
    g.fillStyle = '#0e2a63'
    g.beginPath(); g.arc(x, y, 4.0, 0, 6.3); g.fill()
    g.beginPath(); g.moveTo(x - 2.4, y); g.lineTo(x + 2.4, y); g.lineTo(x + 1.1, y + 8.5); g.lineTo(x - 1.1, y + 8.5); g.closePath(); g.fill()
    g.fillStyle = '#081d49'; g.beginPath(); g.arc(x, y, 2.0, 0, 6.3); g.fill()
  }
  const t = new THREE.CanvasTexture(c)
  t.wrapS = t.wrapT = THREE.RepeatWrapping; t.colorSpace = THREE.SRGBColorSpace; t.anisotropy = 8
  return t
}
function makeChipTexture() {
  const c = document.createElement('canvas')
  c.width = 256; c.height = 256
  const g = c.getContext('2d')!
  g.fillStyle = '#d8c6a2'; g.fillRect(0, 0, 256, 256)
  for (let i = 0; i < 12000; i++) {
    const x = Math.random() * 256, y = Math.random() * 256, d = Math.random()
    g.fillStyle = d > 0.6 ? 'rgba(168,140,96,.28)' : d > 0.3 ? 'rgba(120,96,60,.18)' : 'rgba(225,208,176,.3)'
    g.fillRect(x, y, 1.6, 1.6)
  }
  const t = new THREE.CanvasTexture(c)
  t.wrapS = t.wrapT = THREE.RepeatWrapping; t.repeat.set(1.3, 1.3); t.colorSpace = THREE.SRGBColorSpace; t.anisotropy = 8
  return t
}

function Slot({ loc, selected, onSelect }: { loc: Placed; selected: boolean; onSelect: (id: string) => void }) {
  const [hover, setHover] = useState(false)
  const color = OCCUPANCY_COLORS[loc.occupancyState] ?? OCCUPANCY_COLORS.Occupied
  const empty = loc.occupancyState === 'Available'
  const fill = loc.capacity > 0 ? Math.min(1, loc.sampleCount / loc.capacity) : 0
  const opacity = empty ? 0.14 : 0.4 + 0.4 * fill
  const sampleBoxes = Math.min(loc.sampleCount, 4)

  return (
    <group position={[loc.x, loc.y, loc.z]}>
      <mesh
        castShadow
        onClick={(e) => { e.stopPropagation(); onSelect(loc.id) }}
        onPointerOver={(e) => { e.stopPropagation(); setHover(true) }}
        onPointerOut={() => setHover(false)}
        scale={hover || selected ? 1.04 : 1}
      >
        <boxGeometry args={[SLOT_W * 0.92, SLOT_H * 0.9, SLOT_D * 0.92]} />
        <meshStandardMaterial
          color={color}
          transparent
          opacity={opacity}
          emissive={color}
          emissiveIntensity={selected ? 0.6 : hover ? 0.32 : 0.05}
          metalness={0.1}
          roughness={0.6}
          depthWrite={!empty}
        />
        <Edges visible={selected} scale={1.07} threshold={15} color="#2f6fb3" />
      </mesh>

      {/* stored sample cartons */}
      {Array.from({ length: sampleBoxes }).map((_, i) => (
        <mesh key={i} castShadow position={[-0.3 + i * 0.22, -SLOT_H / 2 + 0.22, 0]}>
          <boxGeometry args={[0.18, 0.32, 0.5]} />
          <meshStandardMaterial color="#c9a56b" roughness={0.85} />
        </mesh>
      ))}

      <Html center distanceFactor={9} position={[0, SLOT_H / 2 + 0.16, 0]} zIndexRange={[10, 0]}>
        <span className={styles.htmlLabel}>{loc.code}</span>
      </Html>
    </group>
  )
}

function RackFrame({ maxLevel, maxPosition, zones, upTex, chipTex }: {
  maxLevel: number; maxPosition: number; zones: string[]; upTex: THREE.Texture; chipTex: THREE.Texture
}) {
  const halfW = (maxPosition * (SLOT_W + GAP_X)) / 2 + 0.2
  const totalH = maxLevel * (SLOT_H + GAP_Y) + BASE_Y
  upTex.repeat.set(1, totalH / 0.52)
  const zoneSpan = SLOT_D + 1.6
  const dz = SLOT_D / 2 + 0.12

  return (
    <>
      {zones.map((zone, zi) => {
        const zc = zi * zoneSpan
        const posts: [number, number][] = [
          [-halfW, zc - dz], [halfW, zc - dz], [-halfW, zc + dz], [halfW, zc + dz],
        ]
        return (
          <group key={zone}>
            {/* blue slotted uprights + black foot caps */}
            {posts.map(([px, pz], i) => (
              <group key={i}>
                <mesh position={[px, totalH / 2, pz]} castShadow receiveShadow>
                  <boxGeometry args={[POST, totalH, POST]} />
                  <meshStandardMaterial map={upTex} metalness={0.3} roughness={0.52} />
                </mesh>
                <mesh position={[px, 0.03, pz]} castShadow>
                  <boxGeometry args={[POST * 1.3, 0.06, POST * 1.3]} />
                  <meshStandardMaterial color="#15171b" metalness={0.2} roughness={0.75} />
                </mesh>
              </group>
            ))}
            {/* yellow step-beams + chipboard decks per shelf */}
            {Array.from({ length: maxLevel }).map((_, L) => {
              const by = L * (SLOT_H + GAP_Y) + BASE_Y
              return (
                <group key={L}>
                  {[zc - dz, zc + dz].map((bz, bi) => (
                    <group key={bi}>
                      <mesh position={[0, by, bz]} castShadow>
                        <boxGeometry args={[halfW * 2, 0.13, 0.05]} />
                        <meshStandardMaterial color="#f4b50c" metalness={0.28} roughness={0.5} />
                      </mesh>
                      <mesh position={[0, by + 0.08, bz + (bz > zc ? 0.012 : -0.012)]} castShadow>
                        <boxGeometry args={[halfW * 2, 0.03, 0.08]} />
                        <meshStandardMaterial color="#f4b50c" metalness={0.28} roughness={0.5} />
                      </mesh>
                    </group>
                  ))}
                  {Array.from({ length: maxPosition }).map((_, p) => {
                    const bx = (p + 1 - (maxPosition + 1) / 2) * (SLOT_W + GAP_X)
                    return (
                      <mesh key={p} position={[bx, by + 0.085, zc]} receiveShadow>
                        <boxGeometry args={[SLOT_W + GAP_X - POST, 0.04, SLOT_D + 0.08]} />
                        <meshStandardMaterial map={chipTex} roughness={0.95} />
                      </mesh>
                    )
                  })}
                </group>
              )
            })}
          </group>
        )
      })}
    </>
  )
}

const CAM_DIR = new THREE.Vector3(0.5, 0.34, 1).normalize()

function CameraRig({ controls, focus, focusKey, resetNonce }: {
  controls: React.RefObject<CameraControlsImpl | null>
  focus: [number, number, number] | null
  focusKey: string | null
  resetNonce: number
}) {
  const { camera } = useThree()
  useEffect(() => {
    const cc = controls.current
    if (!cc) return
    cc.minDistance = 1.5; cc.maxDistance = 24; cc.smoothTime = 0.32
    void cc.setLookAt(...HOME_POS, 0, 0, 0, false)
    cc.saveState()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])
  useEffect(() => {
    const cc = controls.current
    if (!cc || !focus) return
    const [cx, cy, cz] = focus
    const fov = (camera as THREE.PerspectiveCamera).fov
    const size = Math.max(SLOT_W, SLOT_H, SLOT_D)
    const dist = Math.max(2, (size / 2) / Math.tan((fov * Math.PI) / 360) * 1.85)
    const dir = CAM_DIR.clone().multiplyScalar(dist)
    void cc.setLookAt(cx + dir.x, cy + dir.y, cz + dir.z, cx, cy, cz, true)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [focusKey])
  useEffect(() => {
    if (resetNonce > 0) void controls.current?.reset(true)
  }, [resetNonce, controls])
  return null
}

function Scene({ placed, maxLevel, maxPosition, zones, selectedId, onSelect, resetNonce }: {
  placed: Placed[]; maxLevel: number; maxPosition: number; zones: string[]
  selectedId: string | null; onSelect: (id: string) => void; resetNonce: number
}) {
  const controls = useRef<CameraControlsImpl>(null)
  const { upTex, chipTex } = useMemo(() => ({ upTex: makeUprightTexture(), chipTex: makeChipTexture() }), [])
  const totalH = maxLevel * (SLOT_H + GAP_Y) + BASE_Y
  const dz = SLOT_D / 2 + 0.12
  const sel = placed.find((p) => p.id === selectedId)
  const focus: [number, number, number] | null = sel ? [sel.x, sel.y - totalH / 2, sel.z] : null

  // Defer the slot labels by one frame so every <Html> mounts after the canvas
  // DOM target is connected. Without this the very first label (A1-01-10) loses
  // its portal and never appears.
  const [labelsReady, setLabelsReady] = useState(false)
  useEffect(() => {
    const id = requestAnimationFrame(() => setLabelsReady(true))
    return () => cancelAnimationFrame(id)
  }, [])

  return (
    <>
      <color attach="background" args={['#eef1f5']} />
      <hemisphereLight args={[0xffffff, 0x9aa3b0, 0.62]} />
      <ambientLight intensity={0.42} />
      <directionalLight position={[6, 11, 7]} intensity={1.2} castShadow shadow-mapSize={[2048, 2048]} shadow-bias={-0.0003} />
      <directionalLight position={[-7, 6, -4]} intensity={0.3} />

      <group position={[0, -totalH / 2, 0]}>
        <RackFrame maxLevel={maxLevel} maxPosition={maxPosition} zones={zones} upTex={upTex} chipTex={chipTex} />
        {labelsReady && placed.map((loc) => (
          <Slot key={loc.id} loc={loc} selected={selectedId === loc.id} onSelect={onSelect} />
        ))}
        {/* grey floor + white wall, like the real room */}
        <mesh rotation={[-Math.PI / 2, 0, 0]} position={[0, 0, 0]} receiveShadow>
          <planeGeometry args={[60, 60]} />
          <meshStandardMaterial color="#dde3ea" roughness={1} />
        </mesh>
        <mesh position={[0, totalH / 2, -dz - 0.25]} receiveShadow>
          <planeGeometry args={[40, 24]} />
          <meshStandardMaterial color="#f7f8fa" roughness={1} />
        </mesh>
      </group>

      <CameraControls ref={controls} makeDefault maxPolarAngle={Math.PI / 1.9} />
      <CameraRig controls={controls} focus={focus} focusKey={selectedId} resetNonce={resetNonce} />
    </>
  )
}

export function Rack3DPage() {
  const navigate = useNavigate()
  const [params] = useSearchParams()
  const focusCode = params.get('focus')
  const { data, isLoading, error } = useRackOverview()
  const { canWrite } = useSampleTrackingAccess()
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [resetNonce, setResetNonce] = useState(0)

  const placed = useMemo(
    () => (data ? layout(data.locations, data.maxPosition, data.zones) : []),
    [data],
  )

  useEffect(() => {
    if (!focusCode || placed.length === 0) return
    const match = placed.find((p) => p.code.toUpperCase() === focusCode.toUpperCase())
    if (match) setSelectedId(match.id)
  }, [focusCode, placed])

  function backToFull() {
    setSelectedId(null)
    setResetNonce((n) => n + 1)
  }

  return (
    <div className={shared.page}>
      <PageHeader
        title="3D Rack Visualization"
        subtitle="Interactive, database-driven view of the physical QC rack"
        actions={<Button variant="secondary" onClick={() => navigate('/sample-tracking/rack')}><Grid3x3 size={16} /> Grid View</Button>}
      />

      {error && <div className={shared.errorBanner}>Failed to load rack data.</div>}

      <div className={styles.layout}>
        <div className={styles.canvasPanel}>
          <div className={styles.overlayControls}>
            <Button size="sm" variant="secondary" onClick={backToFull}>
              <RotateCcw size={14} /> Reset view
            </Button>
          </div>

          <div className={styles.canvasHost}>
            {data && (
              <Canvas shadows dpr={[1, 2]} camera={{ position: HOME_POS, fov: 45 }}>
                <Suspense fallback={null}>
                  <Scene
                    placed={placed}
                    maxLevel={data.maxLevel}
                    maxPosition={data.maxPosition}
                    zones={data.zones}
                    selectedId={selectedId}
                    onSelect={setSelectedId}
                    resetNonce={resetNonce}
                  />
                </Suspense>
              </Canvas>
            )}
          </div>

          {isLoading && <div className={styles.loading}>Loading rack…</div>}

          <div className={styles.legend}>
            {OCCUPANCY_LEGEND.map((s) => (
              <span className={styles.legendItem} key={s}>
                <span className={styles.legendSwatch} style={{ background: OCCUPANCY_COLORS[s] }} />
                {OCCUPANCY_LABELS[s]}
              </span>
            ))}
          </div>
          <span className={styles.hint}>Click a location to focus · drag to rotate · scroll to zoom</span>
        </div>

        <Rack3DDetail locationId={selectedId} canWrite={canWrite} onBack={selectedId ? backToFull : undefined} />
      </div>
    </div>
  )
}

function Rack3DDetail({ locationId, canWrite, onBack }: { locationId: string | null; canWrite: boolean; onBack?: () => void }) {
  const navigate = useNavigate()
  const { data } = useRackLocationDetail(locationId ?? undefined)

  if (!locationId) {
    return (
      <div className={shared.panel}>
        <p className={shared.muted}>Click a rack location in the 3D view — the camera zooms in and its contents appear here.</p>
      </div>
    )
  }
  if (!data) return <div className={shared.panel}>Loading…</div>

  const full = data.availableCapacity <= 0
  const fillLabel = data.sampleCount === 0 ? 'Empty' : full ? 'Full' : 'Partially occupied'

  return (
    <div className={shared.panel}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: 8 }}>
        <h3 style={{ fontFamily: 'var(--font-mono)', margin: 0, fontSize: 'var(--text-lg)' }}>{data.code}</h3>
        {onBack && <Button size="sm" variant="ghost" onClick={onBack}><Undo2 size={14} /> Back to full rack</Button>}
      </div>
      <p className={shared.muted} style={{ marginTop: 4 }}>
        {OCCUPANCY_LABELS[data.occupancyState]}{!data.isActive && ' · Inactive'}
      </p>
      <div style={{ display: 'flex', gap: 6, flexWrap: 'wrap', margin: '8px 0 4px' }}>
        <span className={shared.muted} style={{ fontSize: 'var(--text-xs)' }}>
          {fillLabel} · {data.sampleCount}/{data.capacity} used · {data.availableCapacity} available
        </span>
      </div>
      {data.samples.length === 0 ? (
        <p className={shared.muted}>No samples stored here.</p>
      ) : (
        <div style={{ marginTop: 8 }}>
          {data.samples.map((s) => (
            <div key={s.id} style={{ padding: '8px 0', borderTop: '1px solid var(--color-border)' }}>
              <button className={shared.linkButton} onClick={() => navigate(`/sample-tracking/samples/${s.id}`)}>
                <span className={shared.mono}>{s.sampleCode}</span>
              </button> — {s.sampleName}
              <div style={{ display: 'flex', gap: 6, marginTop: 4, flexWrap: 'wrap' }}>
                <SampleStatusBadge status={s.status} />
                <AgingBadge category={s.agingCategory} />
              </div>
              <div className={shared.muted} style={{ fontSize: 'var(--text-xs)', marginTop: 2 }}>
                received {formatDate(s.receivedDate)} · {s.totalDaysStored} days
              </div>
            </div>
          ))}
        </div>
      )}
      {canWrite && data.isActive && !full && (
        <div style={{ marginTop: 12 }}>
          <Button fullWidth onClick={() => navigate('/sample-tracking/register')}>Register sample here</Button>
        </div>
      )}
    </div>
  )
}
