import { createBrowserRouter, Navigate, RouterProvider } from 'react-router'
import { AppShell } from '@/app/AppShell'
import { RequireAuth } from '@/app/RequireAuth'
import { CharacterLayout } from '@/pages/character/CharacterLayout'
import { LevelUpPage } from '@/pages/character/LevelUpPage'
import { SheetPage } from '@/pages/character/SheetPage'
import { ShopPage } from '@/pages/character/ShopPage'
import { CharactersPage } from '@/pages/CharactersPage'
import { CreatorPage } from '@/pages/creator/CreatorPage'
import { LoginPage } from '@/pages/LoginPage'

const router = createBrowserRouter([
  {
    element: <AppShell />,
    children: [
      {
        path: '/login',
        element: <LoginPage />,
      },
      {
        element: <RequireAuth />,
        children: [
          {
            path: '/',
            element: (
              <Navigate
                to='/characters'
                replace
              />
            ),
          },
          {
            path: '/characters',
            element: <CharactersPage />,
          },
          {
            path: '/characters/new/:id?',
            element: <CreatorPage />,
          },
          {
            path: '/characters/:id',
            element: <CharacterLayout />,
            children: [
              {
                index: true,
                element: (
                  <Navigate
                    to='sheet/overview'
                    replace
                  />
                ),
              },
              {
                path: 'sheet/:tab',
                element: <SheetPage />,
              },
              {
                path: 'level-up',
                element: <LevelUpPage />,
              },
              {
                path: 'shop',
                element: <ShopPage />,
              },
            ],
          },
        ],
      },
    ],
  },
])

export default function App() {
  return <RouterProvider router={router} />
}
