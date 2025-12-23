import { Autocomplete, Button, Dialog, DialogActions, DialogContent, DialogTitle, Stack, TextField } from "@mui/material";
import { CategoryDto, TagCategoryDto, TagCreateOrUpdateDto, TagDto } from "api/types";
import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { api } from "api";
import { queryKeys } from 'queries';

interface EditTagDialogProps extends EditTagDialogContentProps {
  isOpen: boolean;
}

export function EditTagDialog(props: EditTagDialogProps) {
  const { isOpen, ...restProps } = props;

  return (
    <Dialog open={isOpen} maxWidth="xs" fullWidth>
      <EditTagDialogContent {...restProps} />
    </Dialog>
  );
}

interface EditTagDialogContentProps {
  onClose: () => void;
  categories: CategoryDto[];
  tag: TagDto | null;
}

function EditTagDialogContent(props: EditTagDialogContentProps) {
  const { onClose, categories, tag } = props;

  const [label, setLabel] = useState(tag?.label ?? '');
  const [category, setCategory] = useState<TagCategoryDto | null>(tag?.category ?? null);

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
      await queryClient.invalidateQueries({ queryKey: queryKeys.categories });
      await queryClient.invalidateQueries({ queryKey: queryKeys.tags });
      onClose();
    },
  });

  return (
    <>
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
    </>
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