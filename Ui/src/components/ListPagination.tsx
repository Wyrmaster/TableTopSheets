import {
  Box,
  ButtonGroup,
  Flex,
  IconButton,
  Pagination,
  Text,
} from '@chakra-ui/react'
import { FilterSelect } from '@/components/FilterBar'
import { Icon } from '@/components/Icon'
import { nextIcon, previousIcon } from '@/components/icons'
import { PageSizes } from '@/components/pageSizes'

/**
 * Pages of a list: range ("26–50 of 391"), previous / page numbers / next (only previous, "2 / 16", next on small
 * screens) and, when given a handler, the page size
 */
export const ListPagination = ({
  total,
  page,
  pageSize,
  pageSizeLabel = 'Entries per page',
  onPageChange,
  onPageSizeChange,
}: {
  total: number
  page: number
  pageSize: number
  /** accessible label of the page size select, e.g. `Spells per page` */
  pageSizeLabel?: string
  onPageChange: (page: number) => void
  onPageSizeChange?: (pageSize: number) => void
}) => {
  const first = total === 0 ? 0 : (page - 1) * pageSize + 1
  const last = Math.min(page * pageSize, total)

  return (
    <Flex
      wrap='wrap'
      gap='3'
      align='center'
      justify='space-between'
    >
      <Text
        fontSize='sm'
        color='fg.muted'
      >
        {first}–{last} of {total}
      </Text>
      <Pagination.Root
        count={total}
        pageSize={pageSize}
        page={page}
        siblingCount={1}
        onPageChange={(details) => onPageChange(details.page)}
      >
        <ButtonGroup
          size='sm'
          variant='ghost'
        >
          <Pagination.PrevTrigger asChild>
            <IconButton aria-label='Previous page'>
              <Icon path={previousIcon} />
            </IconButton>
          </Pagination.PrevTrigger>
          {/* page numbers on wider screens, "2 / 16" on phones */}
          <Box display={{
            base: 'none',
            md: 'contents',
          }}
          >
            <Pagination.Items
              render={(item) => (
                <IconButton variant={{
                  base: 'ghost',
                  _selected: 'outline',
                }}
                >
                  {item.value}
                </IconButton>
              )}
            />
          </Box>
          <Pagination.PageText
            format='short'
            hideFrom='md'
            px='2'
          />
          <Pagination.NextTrigger asChild>
            <IconButton aria-label='Next page'>
              <Icon path={nextIcon} />
            </IconButton>
          </Pagination.NextTrigger>
        </ButtonGroup>
      </Pagination.Root>
      {onPageSizeChange && (
        <FilterSelect
          label={pageSizeLabel}
          width='130px'
          value={String(pageSize)}
          onChange={(value) => onPageSizeChange(Number(value))}
        >
          {PageSizes.map((value) => (
            <option
              key={value}
              value={value}
            >
              {value} per page
            </option>
          ))}
        </FilterSelect>
      )}
    </Flex>
  )
}
