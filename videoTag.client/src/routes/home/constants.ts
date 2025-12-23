export const SortBy = {
  LastModified: 'lastModified',
  Title: 'title',
  Size: 'size',
} as const;

export type SortByType = typeof SortBy[keyof typeof SortBy];

export const QueryParam = {
  EditMode: 'editMode',
  TagIds: 'tagIds',
  SortBy: 'sortBy',
} as const;

export enum LocalStorageKey {
  GridItemSize = 'VideoTag-gridItemSize',
}