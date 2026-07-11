export function docSeverity(status: string): 'success' | 'warning' | 'danger' {
  return status === 'Approved' ? 'success' : status === 'Rejected' ? 'danger' : 'warning';
}
