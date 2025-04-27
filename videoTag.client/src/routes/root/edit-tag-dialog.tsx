import { Autocomplete, Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, TextField } from "@mui/material";
import { CategoryDto, TagCreateOrUpdateDto, TagDto } from "api/types";
import { useEffect, useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "api";

interface EditTagDialogProps {
  isOpen: boolean;
  onClose: () => void;
  categories: CategoryDto[];
  tag: TagDto | null;
}

export function EditTagDialog(props: EditTagDialogProps) {
  const {
    isOpen,
    onClose,
    categories,
    tag,
  } = props;

  const [label, setLabel] = useState('');
  const [category, setCategory] = useState<TagDto['category'] | null>(null);

  useEffect(() => {
    if (tag) {
      setLabel(tag.label);
      setCategory(tag.category);
    } else {
      setLabel('');
      setCategory(null);
    }
  }, [tag]);

  const mappedCategories = categories.map(mapCategory);

  const queryClient = useQueryClient();

  const { mutate: createOrUpdateTag } = useMutation({
    mutationFn: () => {
      const dto: TagCreateOrUpdateDto = {
        label,
        categoryId: category!.categoryId,
      };

      if (tag) {
        return api.updateTag(tag.tagId, dto);
      }

      return api.createTag(dto);
    },
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['categories'] });
      await queryClient.invalidateQueries({ queryKey: ['tags'] });
      onClose();
    },
  });

  return (
    <Dialog open={isOpen} maxWidth="xs" fullWidth>
      <DialogTitle>{tag ? 'Edit' : 'Create'} tag</DialogTitle>
      <DialogContent sx={{ overflowY: 'visible' }}>
      <Stack spacing={2}>
        <TextField label="Label" value={label} onChange={e => setLabel(e.target.value)} />
        <CategoryAutocomplete options={mappedCategories} value={category} onChange={value => setCategory(value)} />
      </Stack>
      </DialogContent>
      <DialogActions>
        <Button onClick={() => onClose()}>Cancel</Button>
        <Button
          onClick={() => createOrUpdateTag()}
          disabled={!(label && category)}
          variant="contained"
          disableElevation
        >
          {tag ? 'Update' : 'Create'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}

function mapCategory(category: CategoryDto): TagDto['category'] {
  return ({ categoryId: category.categoryId, label: category.label });
}

interface CategoryAutocompleteProps {
  options: TagDto['category'][];
  value: TagDto['category'] | null;
  onChange: (value: TagDto['category'] | null) => void;
}

function CategoryAutocomplete(props: CategoryAutocompleteProps) {
  const { options, value, onChange } = props;

  return (
    <Autocomplete
      id="category-autocomplete"
      options={options}
      value={value}
      isOptionEqualToValue={(option, value) => {
        return option.categoryId === value.categoryId;
      }}
      onChange={(_, value) => {
        onChange(value);
      }}
      renderInput={(params) => (
        <TextField
          {...params}
          label="Group"
        />
      )}
    />
  );
}