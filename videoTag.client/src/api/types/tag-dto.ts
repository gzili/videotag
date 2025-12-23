export interface TagDto {
  tagId: string;
  label: string;
  category: TagCategoryDto;
}

export interface TagCategoryDto {
  categoryId: string;
  label: string;
}