export interface TagDto {
  tagId: string;
  label: string;
  category: {
    categoryId: string;
    label: string;
  };
}