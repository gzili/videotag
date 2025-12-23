import { Button, Dialog, DialogActions, DialogContent, DialogTitle, TextField } from "@mui/material";
import { CategoryCreateOrUpdateDto, CategoryDto } from "api/types";
import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "api";

interface EditCategoryDialogProps extends EditCategoryDialogContentProps {
  isOpen: boolean;
}

export function EditCategoryDialog(props: EditCategoryDialogProps) {
  const { isOpen, ...restProps } = props;

  return (
    <Dialog open={isOpen} maxWidth="xs" fullWidth>
      <EditCategoryDialogContent {...restProps} />
    </Dialog>
  );
}

interface EditCategoryDialogContentProps {
  onClose: () => void;
  category: CategoryDto | null;
}

function EditCategoryDialogContent(props: EditCategoryDialogContentProps) {
  const { onClose, category } = props;

  const [label, setLabel] = useState(category?.label ?? '');

  const queryClient = useQueryClient();

  const { mutate: createOrUpdateCategory } = useMutation({
    mutationFn: () => {
      const dto: CategoryCreateOrUpdateDto = { label };

      if (category) {
        return api.updateCategory(category.categoryId, dto);
      }

      return api.createCategory(dto);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['categories'] });
      await queryClient.invalidateQueries({ queryKey: ['tags'] });
      onClose();
    },
  });

  return (
    <>
      <DialogTitle>{category ? 'Edit' : 'Create'} group</DialogTitle>
      <DialogContent sx={{ overflowY: 'visible' }}>
        <TextField
          id="category-label-field"
          label="Label"
          value={label}
          onChange={e => setLabel(e.target.value)}
          fullWidth
        />
      </DialogContent>
      <DialogActions>
        <Button onClick={() => onClose()}>Cancel</Button>
        <Button
          onClick={() => createOrUpdateCategory()}
          variant="contained"
          disableElevation
        >
          {category ? 'Update' : 'Create'}
        </Button>
      </DialogActions>
    </>
  );
}