// Routes: Api/TableTopSheets.Api/Endpoints/ImageEndpoints.cs. Images don't need a token (an <img> can't send one);
// the image routes themselves are used as `src`, only the scene list is an intent
import { Intent } from '../core/Intent'
import type { CoinsDto } from '../dtos/inventory'
import type { SceneDto } from '../dtos/images'

/** GET /api/images/coin/{kind}: picture of a coin (`gold`, `silver` ...), as `src` of an image */
export const coinImageUrl = (kind: keyof CoinsDto, thumbnail = true) =>
  `/api/images/coin/${kind}${thumbnail ? '?thumb=true' : ''}`

/** GET /api/images/scenes: the scenes, to show one at random */
export class ListScenesIntent extends Intent<SceneDto[]> {
  readonly method = 'GET'
  override readonly anonymous = true

  /** Route of the intent */
  get path() {
    return '/api/images/scenes'
  }
}
