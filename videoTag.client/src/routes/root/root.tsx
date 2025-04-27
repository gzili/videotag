import { Outlet, ScrollRestoration } from 'react-router-dom';

export const Root = () => (
  <>
    <ScrollRestoration />
    <Outlet />
  </>
);