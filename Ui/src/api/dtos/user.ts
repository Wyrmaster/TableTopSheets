// Source: Api/TableTopSheets.Api/Contracts/UserDtos.cs
import type { Guid, IsoDateTime } from './common'

/** New user of the database authentication */
export interface RegisterDto {
  userName: string
  password: string
  email?: string | null
}

/** Credentials of the database authentication */
export interface LoginDto {
  userName: string
  password: string
}

/** User without credentials */
export interface UserDto {
  id: Guid
  userName: string
  email: string | null
}

/** Signed in user with the number of characters they own */
export interface CurrentUserDto {
  id: Guid
  userName: string
  email: string | null
  characterCount: number
}

/** Access token for the `Authorization: Bearer` header */
export interface TokenDto {
  accessToken: string
  tokenType: string
  expiresAt: IsoDateTime
  user: UserDto
}
