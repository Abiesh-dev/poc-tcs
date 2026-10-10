import type { ReactElement, ReactNode } from 'react';
import { render, type RenderResult } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { ToastProvider } from '../components/toast/ToastProvider';

interface Options {
  route?: string;
}

export const renderWithProviders = (ui: ReactElement, options: Options = {}): RenderResult => {
  const Wrapper = ({ children }: { children: ReactNode }) => (
    <ToastProvider>
      <MemoryRouter initialEntries={[options.route ?? '/']}>{children}</MemoryRouter>
    </ToastProvider>
  );

  return render(ui, { wrapper: Wrapper });
};
