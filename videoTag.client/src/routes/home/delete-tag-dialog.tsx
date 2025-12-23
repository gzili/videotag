import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "api";
import { TagDto } from "api/types";
import { queryKeys } from 'queries';

interface DeleteTagDialogProps {
  isOpen: boolean;
  onClose: () => void;
  tag: TagDto;
}

export function DeleteTagDialog(props: DeleteTagDialogProps) {
  const {
    isOpen,
    onClose,
    tag,
  } = props;

  const queryClient = useQueryClient();

  const { mutate: deleteTag } = useMutation({
    mutationFn: () => api.deleteTag(tag.tagId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.categories });
      await queryClient.invalidateQueries({ queryKey: queryKeys.tags });
      onClose();
    },
  });

  return (
    <Dialog open={isOpen} maxWidth="xs" fullWidth>
      <DialogTitle>Delete tag?</DialogTitle>
      <DialogContent>
        <DialogContentText>
          Tag "{tag.label}" will be deleted.
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={() => onClose()}>Cancel</Button>
        <Button
          color="error"
          variant="contained"
          disableElevation
          onClick={() => deleteTag()}
        >
          Delete
        </Button>
      </DialogActions>
    </Dialog>
  );
}