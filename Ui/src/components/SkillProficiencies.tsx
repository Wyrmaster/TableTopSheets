import { Box, Card, Heading, HStack, Stack, Text } from '@chakra-ui/react'
import { type CharacterSheet, ProficiencyLevels } from '@/api'
import { ProficiencyMarker } from './ProficiencyMarker'

const groups = [
  {
    label: 'Expertise',
    level: ProficiencyLevels.Expertise,
  },
  {
    label: 'Proficient',
    level: ProficiencyLevels.Proficient,
  },
  {
    label: 'Half proficiency',
    level: ProficiencyLevels.Half,
  },
]

/**
 * Skills the character is already proficient in (expertise, proficient, half) and its tools, so choices that grant
 * skills or expertise can be made without opening the sheet
 */
export const SkillProficiencies = ({ sheet }: { sheet: CharacterSheet }) => {
  const skills = sheet.abilities.flatMap((ability) => ability.skills)

  return (
    <Card.Root size='sm'>
      <Card.Body gap='3'>
        <Heading size='sm'>Proficiencies</Heading>
        {groups.map((group) => {
          const names = skills
            .filter((skill) => skill.proficiency === group.level)
            .map((skill) => skill.name)
          return (
            <Box key={group.label}>
              <HStack
                gap='2'
                mb='0.5'
              >
                <ProficiencyMarker level={group.level} />
                <Text
                  fontSize='xs'
                  fontWeight='semibold'
                  color='fg.muted'
                  textTransform='uppercase'
                  letterSpacing='wide'
                >
                  {group.label}
                </Text>
              </HStack>
              <Text
                fontSize='sm'
                color={names.length ? undefined : 'fg.subtle'}
              >
                {names.length ? names.join(', ') : 'none'}
              </Text>
            </Box>
          )
        })}
        <Stack gap='0.5'>
          <Text
            fontSize='xs'
            fontWeight='semibold'
            color='fg.muted'
            textTransform='uppercase'
            letterSpacing='wide'
          >
            Tools
          </Text>
          <Text
            fontSize='sm'
            color={sheet.toolProficiencies.length ? undefined : 'fg.subtle'}
          >
            {sheet.toolProficiencies.length ? sheet.toolProficiencies.join(', ') : 'none'}
          </Text>
        </Stack>
      </Card.Body>
    </Card.Root>
  )
}
