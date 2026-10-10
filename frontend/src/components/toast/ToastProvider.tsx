import { useCallback, useMemo, useRef, type ReactNode } from 'react';
import { Toast } from 'primereact/toast';
import { ToastContext, type ToastTone } from './ToastContext';

const SUMMARY: Record<ToastTone, string> = {
  success: 'Success',
  error: 'Error',
};

export const ToastProvider = ({ children }: { children: ReactNode }) => {
  const toastRef = useRef<Toast>(null);

  const showToast = useCallback((message: string, tone: ToastTone = 'success') => {
    toastRef.current?.show({
      severity: tone,
      summary: SUMMARY[tone],
      detail: message,
      life: 4000,
    });
  }, []);

  const value = useMemo(() => ({ showToast }), [showToast]);

  return (
    <ToastContext.Provider value={value}>
      <Toast ref={toastRef} position="bottom-right" />
      {children}
    </ToastContext.Provider>
  );
};
