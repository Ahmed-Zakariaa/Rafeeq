// Maps a booking status to a PrimeNG Tag severity.
export function bookingSeverity(
  status: string
): 'success' | 'info' | 'warning' | 'danger' | 'secondary' {
  switch (status) {
    case 'Accepted':
    case 'Confirmed':
    case 'Completed':
      return 'success';
    case 'Requested':
      return 'info';
    case 'NoShow':
      return 'warning';
    case 'Rejected':
      return 'danger';
    default: // CancelledByPassenger / CancelledByDriver
      return 'secondary';
  }
}
