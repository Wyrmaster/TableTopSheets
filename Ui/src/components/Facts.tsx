import { Box, SimpleGrid, Text } from '@chakra-ui/react'
import type { ReactNode } from 'react'
import type { TextBlock } from '@/api'
import { RichText } from '@/components/RichText'

/**
 * Label / value grid (spell stats, class facts …)
 */
export const Facts = ({ items }: { items: [string, ReactNode][] }) => {
  return (
    <SimpleGrid
      columns={2}
      gap='1px'
      bg='border'
      borderWidth='1px'
      rounded='md'
      overflow='hidden'
    >
      {items.map(([label, value]) => (
        <Box
          key={label}
          bg='bg'
          px='3'
          py='2'
        >
          <Text
            fontSize='2xs'
            color='fg.muted'
            textTransform='uppercase'
            letterSpacing='wide'
          >
            {label}
          </Text>
          <Text>{value}</Text>
        </Box>
      ))}
    </SimpleGrid>
  )
}

/**
 * Description text of the rules: the structured blocks when the API sent them, otherwise the plain text with line
 * breaks as paragraphs
 */
export const Prose = ({ text, blocks }: { text: string, blocks?: TextBlock[] | null }) => {
  if (blocks && blocks.length > 0) {
    return <RichText blocks={blocks} />
  }

  return (
    <Box
      display='grid'
      gap='2'
      lineHeight='tall'
    >
      {text
        .split('\n')
        .filter((line) => line.trim())
        .map((line, index) => <Text key={index}>{line}</Text>)}
    </Box>
  )
}
