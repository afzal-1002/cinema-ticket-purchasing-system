import { UserSummary } from './auth.model';

export interface UserProfile extends UserSummary {
  phoneNumber: string;
  firstName: string;
  lastName: string;
  id: number;
  version?: number;
}

export interface UpdateUserPayload {
  id: number;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  version?: number;
}
