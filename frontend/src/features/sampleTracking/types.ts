// Types mirroring the SampleTracking API DTOs.

export interface AgingSnapshot {
  receivedDate: string
  currentDate: string
  totalDaysStored: number
  storageAgeMonths: number
  daysRemainingBeforeOverdue: number
  daysOverdue: number
  category: string
}

export interface RackLocation {
  id: string
  code: string
  zone: string
  level: number
  position: number
  description: string | null
  isActive: boolean
  sampleCount: number
  capacity: number
  availableCapacity: number
  hasOverdue: boolean
  hasPendingTesting: boolean
  hasTestingInProgress: boolean
  occupancyState: string
}

export interface RackOccupancy {
  id: string
  code: string
  zone: string
  level: number
  position: number
  isActive: boolean
  sampleCount: number
  capacity: number
  availableCapacity: number
  hasOverdue: boolean
  hasPendingTesting: boolean
  hasTestingInProgress: boolean
  occupancyState: string
}

export interface AgingThresholds {
  newMaxDays: number
  normalMaxDays: number
  overdueDays: number
}

export interface RackOverview {
  zones: string[]
  maxLevel: number
  maxPosition: number
  thresholds: AgingThresholds
  locations: RackOccupancy[]
}

export interface SampleSummary {
  id: string
  sampleCode: string
  sampleName: string
  itemCode: string | null
  model: string | null
  size: string | null
  variant: string | null
  supplierName: string | null
  supplierCode: string | null
  rackLocationId: string | null
  rackLocationCode: string | null
  status: string
  receivedDate: string
  totalDaysStored: number
  daysOverdue: number
  agingCategory: string
  latestTestResult: string | null
}

export interface RackLocationDetail {
  id: string
  code: string
  zone: string
  level: number
  position: number
  description: string | null
  isActive: boolean
  occupancyState: string
  sampleCount: number
  capacity: number
  availableCapacity: number
  samples: SampleSummary[]
}

export interface SampleStatusHistory {
  id: string
  previousStatus: string | null
  newStatus: string
  changedBy: string
  changedAt: string
  reason: string | null
  remarks: string | null
}

export interface SampleMovement {
  id: string
  fromLocationId: string | null
  fromLocationCode: string | null
  toLocationId: string
  toLocationCode: string
  movedBy: string
  movedAt: string
  reason: string | null
  remarks: string | null
}

export interface TestingRecord {
  id: string
  testingType: string
  testingStandard: string | null
  testingDate: string
  testedBy: string
  result: string
  remarks: string | null
  defectDescription: string | null
  photoEvidenceRef: string | null
  reportAttachmentRef: string | null
  createdAt: string
}

export interface SampleAttachment {
  id: string
  fileName: string
  fileRef: string
  contentType: string | null
  sizeBytes: number | null
  category: string
  uploadedBy: string
  uploadedAt: string
}

export interface DisposalRecord {
  id: string
  reason: string
  requestedBy: string
  requestedAt: string
  approvalStatus: string
  approvedBy: string | null
  approvedAt: string | null
  disposalDate: string | null
  remarks: string | null
}

export interface SampleDetail {
  id: string
  sampleCode: string
  sampleName: string
  itemCode: string | null
  productCategory: string | null
  model: string | null
  size: string | null
  variant: string | null
  materialType: string | null
  supplierName: string | null
  supplierCode: string | null
  purchaseOrderNumber: string | null
  grnNumber: string | null
  supplierReference: string | null
  receivedDate: string
  receivedTime: string | null
  receivedQuantity: number
  receivedBy: string | null
  department: string | null
  sampleOwner: string | null
  description: string | null
  purpose: string | null
  testingRequirement: string | null
  requiredTestingType: string | null
  storageRemarks: string | null
  rackLocationId: string | null
  rackLocationCode: string | null
  status: string
  latestTestResult: string | null
  aging: AgingSnapshot
  statusHistory: SampleStatusHistory[]
  movements: SampleMovement[]
  testingRecords: TestingRecord[]
  attachments: SampleAttachment[]
  disposal: DisposalRecord | null
  createdBy: string
  createdAt: string
  modifiedBy: string | null
  modifiedAt: string | null
}

export interface Paged<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

export interface ChartPoint {
  label: string
  value: number
}

export interface OverdueAlert {
  sampleId: string
  sampleCode: string
  sampleName: string
  rackLocationCode: string | null
  receivedDate: string
  totalDaysStored: number
  daysOverdue: number
  agingCategory: string
}

export interface DashboardKpis {
  totalRegisteredSamples: number
  currentlyStoredSamples: number
  availableRackLocations: number
  occupiedRackLocations: number
  pendingTesting: number
  testingInProgress: number
  testedPass: number
  testedFail: number
  overdueSamples: number
  approachingLimit: number
  waitingForSupplier: number
  toBeDisposed: number
}

export interface Dashboard {
  kpis: DashboardKpis
  statusDistribution: ChartPoint[]
  agingDistribution: ChartPoint[]
  rackOccupancy: ChartPoint[]
  monthlyReceiving: ChartPoint[]
  testingPassFail: ChartPoint[]
  samplesBySupplier: ChartPoint[]
  samplesByCategory: ChartPoint[]
  samplesByDepartment: ChartPoint[]
  monthlyMovement: ChartPoint[]
  overdueAlerts: OverdueAlert[]
  recentlyReceived: SampleSummary[]
}

export interface Notification {
  id: string
  type: string
  severity: string
  title: string
  message: string
  sampleId: string | null
  sampleCode: string | null
  isRead: boolean
  createdAt: string
  readAt: string | null
}

export interface NotificationList {
  unreadCount: number
  items: Notification[]
}

export interface SampleSettings {
  companyName: string | null
  departmentName: string | null
  sampleIdPrefix: string
  agingNewMaxDays: number
  agingNormalMaxDays: number
  agingOverdueDays: number
  dateFormat: string
  timeZone: string
  notificationsEnabled: boolean
}
