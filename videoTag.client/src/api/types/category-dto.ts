export interface CategoryDto {
  categoryId: string;
  label: string;
  tags: CategoryTagDto[];
}

export interface CategoryTagDto {
  tagId: string;
  label: string;
  videoCount: number;
}