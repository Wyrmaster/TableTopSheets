// Routes: Api/TableTopSheets.Api/Endpoints/AuthEndpoints.cs, Authentication/DatabaseAuthenticationProvider.cs
import { BodyIntent, Intent } from "../core/Intent"
import type { CurrentUserDto, LoginDto, RegisterDto, TokenDto } from "../dtos/user"

/** POST /api/auth/register: create a user and sign in (database authentication only; 400, 409) */
export class RegisterIntent extends BodyIntent<RegisterDto, TokenDto> {
  readonly method = "POST"
  override readonly anonymous = true

  constructor(body: RegisterDto) {
    super(body)
  }

  get path() {
    return "/api/auth/register"
  }
}

/** POST /api/auth/login: sign in with user name and password (database authentication only; 401) */
export class LoginIntent extends BodyIntent<LoginDto, TokenDto> {
  readonly method = "POST"
  override readonly anonymous = true

  constructor(body: LoginDto) {
    super(body)
  }

  get path() {
    return "/api/auth/login"
  }
}

/** GET /api/users/me: the signed in user */
export class GetCurrentUserIntent extends Intent<CurrentUserDto> {
  readonly method = "GET"

  get path() {
    return "/api/users/me"
  }
}
