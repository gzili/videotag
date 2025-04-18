import { createContext, PropsWithChildren, useContext } from 'react';

interface DialogContextValue {
  onClose: () => void;
}

const DialogContext = createContext<DialogContextValue | undefined>(undefined);

export function useDialogContext() {
  const context = useContext(DialogContext);

  if (!context) {
    throw new Error('useDialogContext must be used within DialogContextProvider');
  }

  return context;
}

interface DialogContextProviderProps extends PropsWithChildren {
  value: DialogContextValue;
}

export function DialogContextProvider(props: DialogContextProviderProps) {
  const { value, children } = props;

  return (
    <DialogContext.Provider value={value}>
      {children}
    </DialogContext.Provider>
  );
}
