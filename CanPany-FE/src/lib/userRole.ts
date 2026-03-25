import type { UserRole } from '@/types';

/**
 * Chuẩn hóa role từ API / localStorage (camelCase, PascalCase, hoặc chữ thường).
 * Tránh Admin bị đẩy sai vì `user.role !== 'Admin'` khi payload là `admin` hoặc `Role`.
 */
export function normalizeAppRole(value: unknown): UserRole | undefined {
  if (value == null || value === '') return undefined;
  const s = String(value).trim();
  switch (s.toLowerCase()) {
    case 'admin':
      return 'Admin';
    case 'company':
      return 'Company';
    case 'candidate':
      return 'Candidate';
    case 'guest':
      return 'Guest';
    default:
      break;
  }
  if (s === 'Admin' || s === 'Company' || s === 'Candidate' || s === 'Guest') {
    return s;
  }
  return undefined;
}

export function isAppRole(value: unknown, expected: UserRole): boolean {
  return normalizeAppRole(value) === expected;
}
