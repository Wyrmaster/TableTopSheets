// Source: Api/TableTopSheets.Api/Contracts/SceneDto.cs

/** Mood picture (tavern, dungeon …) shown at random */
export interface SceneDto {
  key: string
  /** readable name, used as alternative text */
  title: string
  width: number
  height: number
  imageUrl: string
  thumbnailUrl: string
}
