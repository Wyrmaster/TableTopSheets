// Routes: Api/TableTopSheets.Api/Endpoints/AuthEndpoints.cs, Authentication/DatabaseAuthenticationProvider.cs
import { BodyIntent, Intent } from "../core/Intent"
import type { CurrentUserDto, LoginDto, RegisterDto, TokenDto } from "../dtos/user"

/** POST /api/auth/register: create a user and sign in (database authentication only; 400, 409) */
export class RegisterIntent extends BodyIntent<RegisterDto, TokenDto> {
  readonly method = "POST"
  override readonly anonymous = true

  /**
   * @param body user name (3-64 characters), password (at least 8) and optional email
   */
  constructor(body: RegisterDto) {
    super(body)
  }

  /** Route of the intent */
  get path() {
    return "/api/auth/register"
  }
}

/** POST /api/auth/login: sign in with user name and password (database authentication only; 401) */
export class LoginIntent extends BodyIntent<LoginDto, TokenDto> {
  readonly method = "POST"
  override readonly anonymous = true

  /**
   * @param body user name and password
   */
  constructor(body: LoginDto) {
    super(body)
  }

  /** Route of the intent */
  get path() {
    return "/api/auth/login"
  }
}

/** GET /api/users/me: the signed in user (also a cheap way to check whether the token is still valid) */
export class GetCurrentUserIntent extends Intent<CurrentUserDto> {
  readonly method = "GET"

  /** Route of the intent */
  get path() {
    return "/api/users/me"
  }
}
