import { Button, Dialog, DialogActions, DialogContent, DialogTitle, TextField } from "@mui/material";
import { CategoryCreateOrUpdateDto, CategoryDto } from "api/types";
import { useEffect, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "api";

interface EditCategoryDialogProps {
  isOpen: boolean;
  onClose: () => void;
  category: CategoryDto | null;
}

export function EditCategoryDialog(props: EditCategoryDialogProps) {
  const {
    isOpen,
    onClose,
    category,
  } = props;

  const [label, setLabel] = useState('');

  useEffect(() => {
    if (category) {
      setLabel(category.label);
    } else {
      setLabel('');
    }
  }, [category]);

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
    <Dialog open={isOpen} maxWidth="xs" fullWidth>
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
    </Dialog>
  );
}