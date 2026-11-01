import { Box, Heading, List, Span, Table, Text } from '@chakra-ui/react'
import type { TextBlock, TextSpan } from '@/api'

/**
 * Styled runs of text (bold, italic); line breaks inside a span are kept by the parent's `white-space: pre-line`
 */
const Spans = ({ spans }: { spans: TextSpan[] }) => {
  return (
    <>
      {spans.map((span, index) => (
        <Span
          key={index}
          fontWeight={span.bold ? 'semibold' : undefined}
          fontStyle={span.italic ? 'italic' : undefined}
        >
          {span.text}
        </Span>
      ))}
    </>
  )
}

const RichTable = ({ block }: { block: TextBlock }) => {
  const header = block.rows.slice(0, block.headerRows)
  const body = block.rows.slice(block.headerRows)
  return (
    <Table.ScrollArea
      borderWidth='1px'
      rounded='md'
      maxW='full'
    >
      <Table.Root
        size='sm'
        variant='line'
        lineHeight='short'
      >
        {header.length > 0 && (
          <Table.Header bg='bg.subtle'>
            {header.map((row, rowIndex) => (
              <Table.Row
                key={rowIndex}
                bg='bg.subtle'
              >
                {row.map((cell, cellIndex) => (
                  <Table.ColumnHeader
                    key={cellIndex}
                    whiteSpace='pre-line'
                    color='fg.muted'
                    fontWeight='semibold'
                  >
                    <Spans spans={cell} />
                  </Table.ColumnHeader>
                ))}
              </Table.Row>
            ))}
          </Table.Header>
        )}
        <Table.Body>
          {body.map((row, rowIndex) => (
            <Table.Row
              key={rowIndex}
              bg='transparent'
            >
              {row.map((cell, cellIndex) => (
                <Table.Cell
                  key={cellIndex}
                  whiteSpace='pre-line'
                  verticalAlign='top'
                  minW={cellIndex === 0 ? undefined : '8rem'}
                >
                  <Spans spans={cell} />
                </Table.Cell>
              ))}
            </Table.Row>
          ))}
        </Table.Body>
      </Table.Root>
    </Table.ScrollArea>
  )
}

const Block = ({ block }: { block: TextBlock }) => {
  switch (block.kind) {
    case 'Heading':
      return (
        <Heading
          as='h4'
          size='sm'
          fontWeight='semibold'
          whiteSpace='pre-line'
        >
          <Spans spans={block.spans} />
        </Heading>
      )
    case 'List':
      return (
        <List.Root
          as='ul'
          ps='5'
          gap='1'
        >
          {block.items.map((item, index) => (
            <List.Item
              key={index}
              whiteSpace='pre-line'
            >
              <Spans spans={item} />
            </List.Item>
          ))}
        </List.Root>
      )
    case 'Table':
      return <RichTable block={block} />
    default:
      return (
        <Text whiteSpace='pre-line'>
          <Spans spans={block.spans} />
        </Text>
      )
  }
}

/**
 * Structured rule description (paragraphs, headings, lists, tables) as sent in `descriptionBlocks`
 */
export const RichText = ({ blocks }: { blocks: TextBlock[] }) => {
  return (
    <Box
      display='grid'
      gap='3'
      lineHeight='tall'
      minW='0'
    >
      {blocks.map((block, index) => (
        <Block
          key={index}
          block={block}
        />
      ))}
    </Box>
  )
}
