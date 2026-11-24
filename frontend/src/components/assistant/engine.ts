// Shared response engine for the Sample Racking assistant.
// Used by both the floating ChatAssistant widget and the full-screen
// AssistantPage so they answer with one consistent "brain".

export interface Kpis {
  totalRegisteredSamples?: number
  currentlyStoredSamples?: number
  availableRackLocations?: number
  occupiedRackLocations?: number
  pendingTesting?: number
  testingInProgress?: number
  testedPass?: number
  testedFail?: number
  overdueSamples?: number
  approachingLimit?: number
  toBeDisposed?: number
}

export interface AssistantReply {
  text: string
  /** Optional in-app route the reply can deep-link to. */
  to?: string
  toLabel?: string
}

export interface ReplyContext {
  kpis?: Kpis
  firstName: string
}

export function getAssistantReply(input: string, ctx: ReplyContext): AssistantReply {
  const s = input.toLowerCase()
  const k = ctx.kpis
  const link = (text: string, to: string, toLabel: string): AssistantReply => ({ text, to, toLabel })
  const say = (text: string): AssistantReply => ({ text })

  if (/^(hi|hello|hey|yo|salam|hai)\b/.test(s))
    return say(`Hi ${ctx.firstName}! 😊 Ask me about registering samples, the 3D rack, testing, aging, reports, or your live stats.`)
  if (/(thank|thanks|terima kasih|tq)\b/.test(s)) return say("You're welcome! Anything else I can help with?")

  if (/(help|what can you|apa kau boleh|boleh buat apa|options|menu)/.test(s))
    return say(
      'I can help you:\n• Register / receive samples\n• Open the 3D rack & find a location\n• Record testing (pass / fail)\n• Check aging & overdue samples\n• Request disposal\n• Generate reports & QR labels\n• Import from Excel\n• See your live stats\n\nTry: "how do I register a sample" or "show my stats".',
    )

  // ---- Live data ----
  if (/(stat|summary|overview|how many|berapa|total|count)/.test(s)) {
    if (!k) return say('Loading your live stats… please ask again in a moment.')
    return link(
      `Here are your live numbers, ${ctx.firstName}:\n• Total samples: ${k.totalRegisteredSamples ?? 0}\n• Currently stored: ${k.currentlyStoredSamples ?? 0}\n• Overdue (90+ days): ${k.overdueSamples ?? 0}\n• Pending testing: ${k.pendingTesting ?? 0}\n• Tested pass / fail: ${k.testedPass ?? 0} / ${k.testedFail ?? 0}\n• Available locations: ${k.availableRackLocations ?? 0}`,
      '/sample-tracking/dashboard',
      'Open dashboard',
    )
  }
  if (/(overdue|lambat|90)/.test(s))
    return link(`You currently have ${k?.overdueSamples ?? 0} overdue sample(s) (90+ days). Overdue samples are never auto-disposed.`, '/sample-tracking/aging', 'View aging / overdue')

  // ---- How-tos / navigation ----
  if (/(register|receive|add sample|new sample|masuk sample|daftar)/.test(s))
    return link('To register a new sample: open Register / Receive, fill in the details, pick a rack location, and save. A sample code (SMP-…) is generated automatically.', '/sample-tracking/register', 'Go to Register')
  if (/(3d|three|rack view|visuali|tengok rak)/.test(s))
    return link('The 3D rack shows both racks in the room. Click any location to zoom in and see what’s stored there.', '/sample-tracking/rack-3d', 'Open 3D rack')
  if (/(inventory|list|search|find|where|cari|senarai)/.test(s))
    return link('Use Inventory to search and filter all samples by ID, name, supplier, status, or aging — and change a sample’s stage.', '/sample-tracking/inventory', 'Open inventory')
  if (/(test|ujian|pass|fail)/.test(s))
    return link('Record a test from a sample’s detail page (Test button) or quickly change stage from Inventory. Results are Pass or Fail, and aging is never reset by testing.', '/sample-tracking/testing', 'Open testing')
  if (/(aging|age|umur|approach)/.test(s))
    return link('Aging / Overdue tracks how long each sample has been stored, independent of its testing status.', '/sample-tracking/aging', 'View aging')
  if (/(dispose|disposal|buang|lupus)/.test(s))
    return link('Disposal needs an authorized approval. Request it from a sample, then an admin / QC manager approves it on the Disposal page. Disposed samples keep their full history.', '/sample-tracking/disposal', 'Open disposal')
  if (/(move|pindah|stage|status)/.test(s))
    return link('From Inventory, click "Stage" on a row to move a sample to Pending Testing, Under Testing, Done (Pass / Fail), or Request Disposal. To change its location, use Move on the sample’s detail page.', '/sample-tracking/inventory', 'Open inventory')
  if (/(qr|label|sticker|barcode)/.test(s))
    return link('QR Labels lets you generate and print QR codes for rack locations and samples.', '/sample-tracking/labels', 'Open QR labels')
  if (/(report|laporan|export|pdf)/.test(s))
    return link('Reports gives you inventory, aging, testing and disposal reports you can export.', '/sample-tracking/reports', 'Open reports')
  if (/(import|excel|upload|bulk)/.test(s))
    return link('Excel Import lets admins bulk-import samples from a spreadsheet, with validation and preview.', '/sample-tracking/import', 'Open Excel import')
  if (/(user|account|password|role|staff|pekerja)/.test(s))
    return link('Administrators manage accounts and roles under Users — create users, assign roles, reset passwords, and activate / deactivate.', '/sample-tracking/users', 'Open users')
  if (/(setting|config|tetapan)/.test(s))
    return link('Settings (admin) controls aging thresholds, time zone, and module preferences.', '/sample-tracking/settings', 'Open settings')
  if (/(location|rack|bay|shelf|lokasi|unlimited|capacity)/.test(s))
    return link('Locations use the A1-BB-LL code (bay, shelf). Each location holds unlimited samples. Open the 3D rack or Rack Overview to manage them.', '/sample-tracking/rack', 'Open rack overview')

  return say(
    `I’m not sure I caught that, ${ctx.firstName}. I can help with registering samples, the 3D rack, testing, aging, disposal, reports, QR labels, Excel import, users, or your live stats. Try "help" to see everything.`,
  )
}
