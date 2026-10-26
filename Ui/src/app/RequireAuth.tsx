import { Navigate, Outlet, useLocation } from 'react-router'
import { useApi } from '@/api'

/**
 * Pages below need a signed in user; without a token the login page opens and returns here afterwards
 */
export function RequireAuth() {
  const { accessToken } = useApi()
  const location = useLocation()
  if (!accessToken) {
    return (
      <Navigate
        to='/login'
        replace
        state={{ from: location.pathname }}
      />
    )
  }

  return <Outlet />
}
