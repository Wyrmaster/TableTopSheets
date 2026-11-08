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
import {
  clearFilterIcon,
  sortAscendingIcon,
  sortDescendingIcon,
} from '@/components/icons'
import type { MagicFilter } from '@/components/magicFilter'

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

/** Select of the item lists: mundane and magic items, mundane items or magic items */
export const MagicSelect = ({
  value,
  onChange,
}: {
  value: MagicFilter
  onChange: (value: MagicFilter) => void
}) => {
  return (
    <FilterSelect
      label='Magic'
      width='210px'
      value={value}
      onChange={(next) => onChange(next as MagicFilter)}
    >
      <option value=''>Mundane & Magic Items</option>
      <option value='false'>Mundane</option>
      <option value='true'>Magic</option>
    </FilterSelect>
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

/**
 * Sort select of a filter bar with a button next to it that switches between ascending and descending; `value` is the
 * field list of the select (`level,name`), the direction is kept apart (see `sortParam`)
 */
export const SortSelect = ({
  label,
  width,
  value,
  descending,
  onChange,
  onDirectionChange,
  children,
}: {
  label: string
  width: string
  value: string
  descending: boolean
  onChange: (value: string) => void
  onDirectionChange: (descending: boolean) => void
  children: ReactNode
}) => {
  const direction = descending ? 'Descending' : 'Ascending'
  return (
    <HStack gap='1'>
      <FilterSelect
        label={label}
        width={width}
        value={value}
        onChange={onChange}
      >
        {children}
      </FilterSelect>
      <IconButton
        variant='outline'
        aria-label={direction}
        title={direction}
        aria-pressed={descending}
        onClick={() => onDirectionChange(!descending)}
      >
        <Icon path={descending ? sortDescendingIcon : sortAscendingIcon} />
      </IconButton>
    </HStack>
  )
}
