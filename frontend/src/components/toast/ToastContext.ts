import { createContext, useContext } from 'react';

export type ToastTone = 'success' | 'error';

export interface ToastApi {
  showToast: (message: string, tone?: ToastTone) => void;
}

export const ToastContext = createContext<ToastApi | undefined>(undefined);

export const useToast = (): ToastApi => {
  const context = useContext(ToastContext);

  if (!context) {
    throw new Error('useToast must be used inside a ToastProvider.');
  }

  return context;
};
