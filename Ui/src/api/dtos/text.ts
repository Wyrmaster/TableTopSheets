// Sources: Common/Sheet/TextBlock.cs, Common/Sheet/TextSpan.cs, Common/Enum/TextBlockKindEnum.cs

/** Kind of a block of a rule description */
export type TextBlockKindEnum = 'Paragraph' | 'Heading' | 'List' | 'Table'

/** Run of text with one style inside a block; `text` may contain "\n" (a line break) */
export interface TextSpan {
  text: string
  bold: boolean
  italic: boolean
}

/** Block of a rule description (paragraph, heading, list or table) */
export interface TextBlock {
  kind: TextBlockKindEnum
  /** text of a paragraph or heading */
  spans: TextSpan[]
  /** entries of a list */
  items: TextSpan[][]
  /** rows of a table, each a list of cells */
  rows: TextSpan[][][]
  /** leading rows of `rows` that are the header of the table */
  headerRows: number
}
