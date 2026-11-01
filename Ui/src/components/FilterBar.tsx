import {
  Checkbox,
  Flex,
  HStack,
  IconButton,
  Input,
  NativeSelect,
  Stack,
} from '@chakra-ui/react'
import type { ReactNode } from 'react'
import { Icon } from '@/components/Icon'
import { clearFilterIcon } from '@/components/icons'

/**
 * Filter bar of a list: the search box and a "clear filters" icon button on the first line, every other filter
 * (`children`: selects, toggles, sort) on the next one, wrapping on narrow screens
 */
export const FilterBar = ({
  search,
  placeholder,
  filtered,
  onSearchChange,
  onClear,
  children,
}: {
  search: string
  placeholder: string
  /** whether any filter is set (enables the clear button) */
  filtered: boolean
  onSearchChange: (search: string) => void
  onClear: () => void
  children: ReactNode
}) => {
  return (
    <Stack gap='3'>
      <HStack gap='2'>
        <Input
          flex='1'
          minW='0'
          placeholder={placeholder}
          aria-label='Search'
          value={search}
          onChange={(event) => onSearchChange(event.target.value)}
        />
        <IconButton
          variant='outline'
          aria-label='Clear filters'
          title='Clear filters'
          disabled={!filtered}
          onClick={onClear}
        >
          <Icon path={clearFilterIcon} />
        </IconButton>
      </HStack>
      <Flex
        wrap='wrap'
        gap='3'
        align='center'
      >
        {children}
      </Flex>
    </Stack>
  )
}

/** Native select of a filter bar; `label` is its accessible name */
export const FilterSelect = ({
  label,
  width,
  value,
  disabled,
  onChange,
  children,
}: {
  label: string
  width: string
  value: string
  disabled?: boolean
  onChange: (value: string) => void
  children: ReactNode
}) => {
  return (
    <NativeSelect.Root
      w={width}
      disabled={disabled}
    >
      <NativeSelect.Field
        aria-label={label}
        value={value}
        onChange={(event) => onChange(event.target.value)}
      >
        {children}
      </NativeSelect.Field>
      <NativeSelect.Indicator />
    </NativeSelect.Root>
  )
}

/** Checkbox of a filter bar */
export const FilterToggle = ({
  checked,
  onChange,
  children,
}: {
  checked: boolean
  onChange: (checked: boolean) => void
  children: ReactNode
}) => {
  return (
    <Checkbox.Root
      checked={checked}
      colorPalette='brand'
      onCheckedChange={(details) => onChange(details.checked === true)}
    >
      <Checkbox.HiddenInput />
      <Checkbox.Control />
      <Checkbox.Label>{children}</Checkbox.Label>
    </Checkbox.Root>
  )
}
