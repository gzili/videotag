import ky from "ky";
import { API_HOST } from "../env.ts";

export const api = ky.create({
  prefixUrl: `${API_HOST}/api`,
  retry: 0,
});