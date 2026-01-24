export interface LoginPayload {
  email: string;
  password: string;
}

export interface RegisterPayload {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  phoneNumber: string;
}

export interface UserSummary {
  id?: number;
  email: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
  isAdmin?: boolean;
  version?: number;
}

export interface AuthResponse {
  token: string;
  user?: UserSummary;
}
