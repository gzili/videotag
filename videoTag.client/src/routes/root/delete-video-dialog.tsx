import { Box, Button, Checkbox, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, FormControlLabel } from "@mui/material";
import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";

import { api } from "../../api";
import { VideoListItemDto } from "../../api/types";

interface DeleteVideoDialogProps {
  isOpen: boolean;
  onClose: () => void;
  video: VideoListItemDto;
}

export function DeleteVideoDialog(props: DeleteVideoDialogProps) {
  const { isOpen, onClose, video } = props;
  
  const [keepFileOnDisk, setKeepFileOnDisk] = useState(false);
  
  const queryClient = useQueryClient();
  
  const { mutate: deleteVideo } = useMutation({
    mutationFn: () => api.deleteVideo(video.videoId, keepFileOnDisk),
    onSuccess: () => {
      onClose();
      queryClient.invalidateQueries({
        queryKey: ['videos'],
      });
    },
  });
  
  return (
    <Dialog
      open={isOpen}
      maxWidth="xs"
      fullWidth
    >
      <DialogTitle>Remove video?</DialogTitle>
      <DialogContent>
        <DialogContentText>Video "{video.title}" will be removed from the library.</DialogContentText>
        <Box>
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
          Delete
        </Button>
      </DialogActions>
    </Dialog>
  );
}