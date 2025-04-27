import { useMutation, useQueryClient } from "@tanstack/react-query";
import { CategoryDto } from "api/types";
import { api } from "api";
import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";

interface DeleteCategoryDialogProps {
  isOpen: boolean;
  onClose: () => void;
  category: CategoryDto;
}

export function DeleteCategoryDialog(props: DeleteCategoryDialogProps) {
  const {
    isOpen,
    onClose,
    category,
  } = props;

  const queryClient = useQueryClient();

  const { mutate: deleteCategory } = useMutation({
    mutationFn: () => api.deleteCategory(category.categoryId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['categories'] });
      await queryClient.invalidateQueries({ queryKey: ['tags'] });
      onClose();
    },
  });

  return (
    <Dialog open={isOpen} maxWidth="xs" fullWidth>
      <DialogTitle>Delete group?</DialogTitle>
      <DialogContent>
        <DialogContentText>
          Group "{category.label}" will be permanently deleted. 
        </DialogContentText>
      </DialogContent>
      <DialogActions>
        <Button onClick={() => onClose()}>Cancel</Button>
        <Button
          color="error"
          variant="contained"
          disableElevation
          onClick={() => deleteCategory()}
        >
          Delete
        </Button>
      </DialogActions>
    </Dialog>
  );
}