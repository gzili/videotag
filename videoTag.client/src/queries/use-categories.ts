import { useQuery } from '@tanstack/react-query';
import { api } from 'api';
import { queryKeys } from './query-keys';

export const useCategories = () => {
  return useQuery({
    queryKey: queryKeys.categories,
    queryFn: () => api.getCategories(),
    select: categories => {
      const otherCategoryIndex = categories.findIndex(c => c.categoryId === '00000000-0000-0000-0000-000000000000');
      if (otherCategoryIndex) {
        categories.push(categories.splice(otherCategoryIndex, 1)[0]);
      }
      return categories;
    },
    staleTime: Infinity,
    gcTime: Infinity,
  });
}