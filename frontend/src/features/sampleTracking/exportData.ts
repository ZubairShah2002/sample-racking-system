import * as XLSX from 'xlsx'

/** Triggers a browser download of a Blob. */
function download(blob: Blob, filename: string) {
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = filename
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(url)
}

export function exportRows(rows: Record<string, unknown>[], filename: string, format: 'xlsx' | 'csv') {
  const ws = XLSX.utils.json_to_sheet(rows)
  if (format === 'csv') {
    const csv = XLSX.utils.sheet_to_csv(ws)
    download(new Blob([csv], { type: 'text/csv;charset=utf-8;' }), `${filename}.csv`)
    return
  }
  const wb = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(wb, ws, 'Data')
  const out = XLSX.write(wb, { bookType: 'xlsx', type: 'array' })
  download(new Blob([out], { type: 'application/octet-stream' }), `${filename}.xlsx`)
}

/** Parses the first sheet of an uploaded spreadsheet into row objects. */
export async function parseWorkbook(file: File): Promise<Record<string, unknown>[]> {
  const buf = await file.arrayBuffer()
  const wb = XLSX.read(buf, { type: 'array' })
  const first = wb.SheetNames[0]
  if (!first) return []
  return XLSX.utils.sheet_to_json<Record<string, unknown>>(wb.Sheets[first], { defval: '' })
}
