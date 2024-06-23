export interface CategoryDto {
  categoryId: string;
  label: string;
  tags: {
    tagId: string;
    label: string;
  }[];
}