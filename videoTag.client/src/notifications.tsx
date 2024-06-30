import { Box, IconButton, Paper, Typography } from '@mui/material';
import LinearProgress from '@mui/material/LinearProgress';
import { useCallback, useEffect, useState } from 'react';
import CloseIcon from '@mui/icons-material/Close';
import * as signalR from "@microsoft/signalr";
import { API_HOST } from './env';
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

export function Notifications() {
  const [title, setTitle] = useState<string | null>(null);
  const [progress, setProgress] = useState<ProgressState | null>(null);

  const handleClose = useCallback(() => {
    setTitle(null);
    setProgress(null);
  }, []);

  const queryClient = useQueryClient();

  useEffect(() => {
    const eventHandlers = [
      {
        event: HubEvents.SyncStarted,
        handler: () => {
          setTitle('Sync started');
          setProgress(null);
        },
      },
      {
        event: HubEvents.SyncProgress,
        handler: (fileName: string, current: number, total: number) => {
          setTitle('Sync in progress');
          setProgress({ fileName, current, total });
        },
      },
      {
        event: HubEvents.SyncFinished,
        handler: () => {
          setTitle('Sync finished');
          setProgress(null);
          queryClient.invalidateQueries({
            queryKey: ['videos'],
          });
        },
      },
      {
        event: HubEvents.SyncFailed,
        handler: () => {
          setTitle('Sync failed');
          setProgress(null);
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

  if (title === null) {
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
          {title}
        </Typography>
        {progress && (
          <>
            <Typography fontSize="0.8rem" overflow="hidden" sx={{ wordWrap: 'break-word' }} pt="0.2rem" pb="0.4rem">
              {progress.fileName}
            </Typography>
            <Box>
              <LinearProgress variant="determinate" value={Math.floor(progress.current / progress.total * 100)} />
            </Box>
            <Typography fontSize="0.8rem" color="grey.600" pt="0.4rem">
              {progress.current} of {progress.total}
            </Typography>
          </>
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
