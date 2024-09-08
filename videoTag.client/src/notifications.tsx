import { Box, IconButton, Paper, Typography } from '@mui/material';
import LinearProgress from '@mui/material/LinearProgress';
import { useCallback, useEffect, useState } from 'react';
import CloseIcon from '@mui/icons-material/Close';
import * as signalR from "@microsoft/signalr";
import { API_HOST } from 'env';
import { useQueryClient } from '@tanstack/react-query';

const HubEvents = {
  SyncStarted: 'syncStarted',
  SyncProgress: 'syncProgress',
  SyncFinished: 'syncFinished',
  SyncFailed: 'syncFailed',
} as const;

const connection = new signalR.HubConnectionBuilder()
    .withUrl(`${API_HOST}/hub`, { withCredentials: false })
    .build();

interface ProgressState {
  fileName: string;
  current: number;
  total: number;
}

interface NotificationState {
  title?: string;
  description?: string;
  progress?: ProgressState;
}

export function Notifications() {
  const [notificationState, setNotificationState] = useState<NotificationState>({});

  const handleClose = useCallback(() => setNotificationState({}), []);

  const queryClient = useQueryClient();

  useEffect(() => {
    const eventHandlers = [
      {
        event: HubEvents.SyncStarted,
        handler: () => {
          setNotificationState({ title: 'Sync started' })
        },
      },
      {
        event: HubEvents.SyncProgress,
        handler: (fileName: string, current: number, total: number) => {
          setNotificationState({
            title: 'Sync in progress',
            progress: { fileName, current, total },
          });
        },
      },
      {
        event: HubEvents.SyncFinished,
        handler: (numAdded: number, numUpdated: number, numRemoved: number) => {
          setNotificationState({
            title: 'Sync finished',
            description: `Added ${numAdded} entries. Updated ${numUpdated} entries. Removed ${numRemoved} entries.`,
          })
          queryClient.invalidateQueries({
            queryKey: ['videos'],
          });
        },
      },
      {
        event: HubEvents.SyncFailed,
        handler: () => {
          setNotificationState({ title: 'Sync failed' })
        },
      },
    ];

    for (const eventHandler of eventHandlers) {
      connection.on(eventHandler.event, eventHandler.handler);
    }

    return () => {
      for (const eventHandler of eventHandlers) {
        connection.off(eventHandler.event, eventHandler.handler);
      }
    };
  }, [queryClient]);

  useEffect(() => {
    if (connection.state === signalR.HubConnectionState.Disconnected) {
      connection.start().catch((error) => { console.log(error) });
    }
  }, []);

  if (!notificationState.title) {
    return null;
  }
  
  return (
    <Paper
      variant="outlined"
      sx={{
        position: 'fixed',
        zIndex: 999,
        right: '1rem',
        bottom: '1rem',
        p: '0.8rem',
        display: 'flex',
        alignItems: 'center',
      }}
    >
      <Box width="400px">
        <Typography fontWeight="bold">
          {notificationState.title}
        </Typography>
        {notificationState.progress && (
          <>
            <Typography fontSize="0.8rem" overflow="hidden" sx={{ wordWrap: 'break-word' }} pt="0.2rem" pb="0.4rem">
              {notificationState.progress.fileName}
            </Typography>
            <Box>
              <LinearProgress
                variant="determinate"
                value={Math.floor(notificationState.progress.current / notificationState.progress.total * 100)}
              />
            </Box>
            <Typography fontSize="0.8rem" color="grey.600" pt="0.4rem">
              {notificationState.progress.current} of {notificationState.progress.total}
            </Typography>
          </>
        )}
        {notificationState.description && (
          <Typography fontSize="0.8rem" pt="0.2rem">
            {notificationState.description}
          </Typography>
        )}
      </Box>
      <Box pl="0.8rem">
        <IconButton aria-label="close" size="small" onClick={handleClose}>
          <CloseIcon fontSize="inherit" />
        </IconButton>
      </Box>
    </Paper>
  );
}
