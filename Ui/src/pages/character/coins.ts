import type { CoinsDto } from '@/api'

/** Coin kinds with their value in copper pieces, highest first */
export const CoinKinds: {
  key: keyof CoinsDto,
  label: string,
  short: string,
  value: number
}[] = [
  {
    key: 'platinum',
    label: 'Platinum',
    short: 'pp',
    value: 1000,
  },
  {
    key: 'gold',
    label: 'Gold',
    short: 'gp',
    value: 100,
  },
  {
    key: 'electrum',
    label: 'Electrum',
    short: 'ep',
    value: 50,
  },
  {
    key: 'silver',
    label: 'Silver',
    short: 'sp',
    value: 10,
  },
  {
    key: 'copper',
    label: 'Copper',
    short: 'cp',
    value: 1,
  },
]

/** Value of the coins in copper pieces */
export const coinValue = (coins: CoinsDto) => CoinKinds.reduce((sum, kind) => sum + coins[kind.key] * kind.value, 0)

/** The coins as text, e.g. `2 pp · 15 gp · 4 sp` ("no coins" when empty) */
export function formatCoins(coins: CoinsDto) {
  const parts = CoinKinds.filter((kind) => coins[kind.key]).map((kind) => `${coins[kind.key]} ${kind.short}`)
  return parts.length ? parts.join(' · ') : 'no coins'
}

/** A price in copper pieces as text, e.g. `15 gp`, `5 sp`, `2 cp` */
export function formatPrice(cp: number) {
  const parts = [
    cp >= 100 && `${Math.floor(cp / 100)} gp`,
    cp % 100 >= 10 && `${Math.floor((cp % 100) / 10)} sp`,
    cp % 10 > 0 && `${cp % 10} cp`,
  ].filter(Boolean)
  return parts.length ? parts.join(' ') : '0 cp'
}
