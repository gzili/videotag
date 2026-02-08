import { Box, Button, Checkbox, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, FormControlLabel } from "@mui/material";
import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";

import { api } from "../api";
import { queryKeys } from 'queries';

interface DeleteVideoDialogProps extends DeleteVideoDialogContentProps {
  isOpen: boolean;
}

export function DeleteVideoDialog(props: DeleteVideoDialogProps) {
  const { isOpen, onClose, ...contentProps } = props;
  
  return (
    <Dialog
      open={isOpen}
      maxWidth="sm"
      fullWidth
    >
      <DeleteVideoDialogContent onClose={onClose} {...contentProps} />
    </Dialog>
  );
}

interface DeleteVideoDialogContentProps {
  videoId: string;
  videoTitle: string;
  onClose: () => void;
  onSuccess?: () => void;
}

function DeleteVideoDialogContent(props: DeleteVideoDialogContentProps) {
  const { onClose, videoId, videoTitle, onSuccess } = props;

  const [keepFileOnDisk, setKeepFileOnDisk] = useState(false);
  
  const queryClient = useQueryClient();
  
  const { mutate: deleteVideo } = useMutation({
    mutationFn: () => api.deleteVideo(videoId, keepFileOnDisk),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.videos() });
      queryClient.invalidateQueries({ queryKey: queryKeys.categories });
      onClose();
      onSuccess?.();
    },
  });

  return (
    <>
      <DialogTitle>Remove video?</DialogTitle>
      <DialogContent>
        <DialogContentText>Video "<b>{videoTitle}</b>" will be removed from the library.</DialogContentText>
        <Box pt="0.2rem">
          <FormControlLabel
            control={<Checkbox checked={keepFileOnDisk} onChange={e => setKeepFileOnDisk(e.target.checked)} />}
            label="Keep file on disk"
          />
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          color="error"
          variant="contained"
          disableElevation
          onClick={() => { deleteVideo() }}
        >
          Remove
        </Button>
      </DialogActions>
    </>
  );
}
