import { Box, SimpleGrid, Text } from '@chakra-ui/react'
import type { ReactNode } from 'react'

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
 * Description text of the rules; line breaks become paragraphs
 */
export const Prose = ({ text }: { text: string }) => {
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
