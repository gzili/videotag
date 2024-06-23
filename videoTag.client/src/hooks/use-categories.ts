import { useQuery } from "@tanstack/react-query";
import { api } from "../api";

export function useCategories(includeTags?: boolean) {
  const { data } = useQuery({
    queryKey: ['categories'],
    queryFn: () => api.getCategories(includeTags),
    select: categories => {
      const otherCategoryIndex = categories.findIndex(c => c.categoryId === '00000000-0000-0000-0000-000000000000');
      if (otherCategoryIndex) {
        categories.push(categories.splice(otherCategoryIndex, 1)[0]);
      }
      return categories;
    }
  });

  return ({ categories: data });
}
