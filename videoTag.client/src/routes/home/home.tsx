import DeleteIcon from '@mui/icons-material/Delete';
import EditIcon from '@mui/icons-material/Edit';
import FolderIcon from '@mui/icons-material/Folder';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import SellIcon from '@mui/icons-material/Sell';
import SyncIcon from '@mui/icons-material/Sync';
import {
  Box,
  Button,
  Chip,
  FormControl,
  FormControlLabel,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  Stack,
  Switch,
  Typography
} from "@mui/material";
import { useCallback, useMemo, useState } from "react";
import { Link, useSearchParams } from "react-router-dom";

import { api } from "api";
import { CategoryDto, TagDto, VideoListItemDto } from 'api/types';
import { DeleteVideoDialog } from 'components';
import { API_HOST } from "env.ts";
import { useCategories } from 'hooks';
import { formatDuration, formatSize } from "utils.ts";

import { QueryParam, SortBy, SortByType } from './constants.ts';
import { DeleteCategoryDialog } from './delete-category-dialog.tsx';
import { DeleteTagDialog } from './delete-tag-dialog.tsx';
import { EditCategoryDialog } from './edit-category-dialog.tsx';
import { EditTagDialog } from './edit-tag-dialog.tsx';
import { useVideos } from "./hooks.ts";

export function Home() {
  return (
    <Box px={2}>
      <Tags />
      <Videos />
    </Box>
  );
}

export function Tags() {
  const [searchParams, setSearchParams] = useSearchParams();

  const editMode = searchParams.get(QueryParam.EditMode) !== null;

  const handleEditModeChange = (isEnabled: boolean) => {
    if (isEnabled) {
      setSearchParams(params => {
        params.delete(QueryParam.TagIds);
        params.set(QueryParam.EditMode, '1');
        return params;
      });
    } else {
      setSearchParams(params => {
        params.delete(QueryParam.EditMode);
        return params;
      });
    }
  }

  const tagIds = searchParams.getAll(QueryParam.TagIds);
  const [isEditTagDialogOpen, setIsEditTagDialogOpen] = useState(false);
  const [isDeleteTagDialogOpen, setIsDeleteTagDialogOpen] = useState(false);
  const [tag, setTag] = useState<TagDto | null>(null);

  function getTagClickHandler() {
    if (editMode) {
      return (tag: CategoryDto['tags'][0], category: CategoryDto) => {
        setTag(tagInCategoryToTagDto(tag, category));
        setIsEditTagDialogOpen(true);
      };
    } else {
      return (tag: CategoryDto['tags'][0]) => {
        if (tagIds.includes(tag.tagId)) {
          searchParams.delete(QueryParam.TagIds, tag.tagId);
        } else {
          searchParams.append(QueryParam.TagIds, tag.tagId);
        }
    
        setSearchParams(searchParams);
      };
    }
  }

  const handleTagClick = getTagClickHandler();

  const [isEditCategoryDialogOpen, setIsEditCategoryDialogOpen] = useState(false);
  const [isDeleteCategoryDialogOpen, setIsDeleteCategoryDialogOpen] = useState(false);
  const [category, setCategory] = useState<CategoryDto | null>(null);

  const { categories } = useCategories(true);

  if (categories === undefined) {
    return null;
  }

  return (
    <Box pt={1}>
      <Box display="flex" alignItems="center">
        <Typography fontSize={24} fontWeight="bold">Tags</Typography>
        <Box flex="1">
          {editMode && (
            <Stack direction="row" spacing={1} pl="1rem">
              <Button
                size="small"
                variant="contained"
                startIcon={<FolderIcon />}
                disableElevation
                onClick={() => {
                  setCategory(null);
                  setIsEditCategoryDialogOpen(true);
                }}
              >
                Create group
              </Button>
              <Button 
                size="small"
                variant="contained"
                startIcon={<SellIcon />}
                disableElevation
                onClick={() => {
                  setTag(null);
                  setIsEditTagDialogOpen(true);
                }}
              >
                Create tag
              </Button>
            </Stack>
          )}
        </Box>
        <Box>
          <FormControlLabel
            control={<Switch checked={editMode} onChange={e => handleEditModeChange(e.target.checked)} />}
            label="Edit mode"
            sx={{ mr: 0 }}
          />
        </Box>
      </Box>
      <Stack spacing={1} pt={1}>
        {categories.map(c => (
          <Box key={c.categoryId}>
            <Box display="flex" alignItems="center">
              <Typography fontWeight="bold">{c.label}</Typography>
              {editMode && (
                <Stack direction="row" pl="6px">
                  <IconButton
                    aria-label="Edit category"
                    size="small"
                    onClick={() => {
                      setCategory(c);
                      setIsEditCategoryDialogOpen(true);
                    }}
                  >
                    <EditIcon fontSize="inherit" />
                  </IconButton>
                  <IconButton
                    aria-label="Delete category"
                    size="small"
                    onClick={() => {
                      setCategory(c);
                      setIsDeleteCategoryDialogOpen(true);
                    }}
                  >
                    <DeleteIcon fontSize="inherit" />
                  </IconButton>
                </Stack>
              )}
            </Box>
            <Box maxWidth="1500px" pt="2px" display="flex" flexWrap="wrap" gap={1}>
              {c.tags.map(t => (
                <Chip
                  key={t.tagId}
                  label={t.label}
                  color={tagIds.includes(t.tagId) ? "primary" : undefined}
                  onClick={() => handleTagClick(t, c)}
                  onDelete={editMode ? (
                    () => {
                      setTag(tagInCategoryToTagDto(t, c));
                      setIsDeleteTagDialogOpen(true);
                    }
                  ) : undefined}
                />
              ))}
              {c.tags.length === 0 && (
                <Typography color="grey.600" fontStyle="italic" fontSize="0.8125rem">No tags.</Typography>
              )}
            </Box>
          </Box>
        ))}
      </Stack>
      <EditCategoryDialog
        isOpen={isEditCategoryDialogOpen}
        onClose={() => setIsEditCategoryDialogOpen(false)}
        category={category}
      />
      {category && (
        <DeleteCategoryDialog
          isOpen={isDeleteCategoryDialogOpen}
          onClose={() => setIsDeleteCategoryDialogOpen(false)}
          category={category}
        />
      )}
      <EditTagDialog
        isOpen={isEditTagDialogOpen}
        onClose={() => setIsEditTagDialogOpen(false)}
        categories={categories}
        tag={tag}
      />
      {tag && (
        <DeleteTagDialog
          isOpen={isDeleteTagDialogOpen}
          onClose={() => setIsDeleteTagDialogOpen(false)}
          tag={tag}
        />
      )}
    </Box>
  );
}

function tagInCategoryToTagDto(tag: CategoryDto['tags'][0], category: CategoryDto) {
  return ({
    tagId: tag.tagId,
    label: tag.label,
    category: {
      categoryId: category.categoryId,
      label: category.label,
    },
  });
}

function Videos() {
  const [searchParams, setSearchParams] = useSearchParams();

  const sortBy = searchParams.get('sortBy') || SortBy.LastModified;

  const setSortBy = useCallback((sortBy: string) => {
    setSearchParams(searchParams => {
      searchParams.set(QueryParam.SortBy, sortBy);
      return searchParams;
    })
  }, [setSearchParams]);
  
  const { videos } = useVideos();
    
  useMemo(() => {
    if (videos !== undefined) {
      switch (sortBy) {
        case SortBy.LastModified:
          videos.sort((a, b) => b.lastModifiedUnixSeconds - a.lastModifiedUnixSeconds);
          break;
        case SortBy.Title:
          videos.sort((a, b) => a.title.localeCompare(b.title, 'en', { numeric: true }));
          break;
        case SortBy.Size:
          videos.sort((a, b) => b.size - a.size);
          break;
      }
    }
  }, [videos, sortBy]);
  
  const [isVideoDeleteDialogOpen, setIsVideoDeleteDialogOpen] = useState(false);
  const [video, setVideo] = useState<VideoListItemDto | null>(null);

  if (videos === undefined) {
    return null;
  }
  
  return (
    <Box pt={2}>
      <Box display="flex" alignItems="center">
        <Typography fontSize={24} fontWeight="bold">
          {searchParams.getAll('tagIds').length === 0 && 'Untagged '}Videos ({videos.length})
        </Typography>
        <Box flex="1" pl="1rem">
          <Button
            size="small"
            variant="contained"
            disableElevation
            startIcon={<SyncIcon />}
            onClick={() => api.syncVideos()}
          >
            Sync
          </Button>
        </Box>
        <FormControl>
          <InputLabel id="sort-by-label">Sort by</InputLabel>
          <Select
            size="small"
            value={sortBy}
            onChange={e => setSortBy(e.target.value as SortByType)}
            labelId="sort-by-label"
            label="Sort by"
          >
            <MenuItem value={SortBy.LastModified}>Last modified</MenuItem>
            <MenuItem value={SortBy.Title}>Title</MenuItem>
            <MenuItem value={SortBy.Size}>Size</MenuItem>
          </Select>
        </FormControl>
      </Box>
      <Box pt={1} display="grid" gridTemplateColumns="repeat(auto-fill, minmax(200px, 1fr))" gap={1}>
        {videos.map(video => (
          <VideoCard
            key={video.videoId}
            video={video}
            onVideoToDeleteChange={video => {
              setVideo(video);
              setIsVideoDeleteDialogOpen(true);
            }}
          />
        ))}
      </Box>
      {video && (
        <DeleteVideoDialog
          isOpen={isVideoDeleteDialogOpen}
          onClose={() => setIsVideoDeleteDialogOpen(false)}
          videoId={video.videoId}
          videoTitle={video.title}
        />
      )}
    </Box>
  );
}

interface VideoCardProps {
  video: VideoListItemDto;
  onVideoToDeleteChange: (video: VideoListItemDto) => void;
}

function VideoCard(props: VideoCardProps) {
  const {video, onVideoToDeleteChange } = props;

  return (
    <div className="video-item">
      <div className="__thumbnail">
        <img src={API_HOST + video.thumbnailUrl} loading="lazy" />
        <div className="__overlay-badge __resolution">{formatResolution(video.width, video.height)}</div>
        <div className="__overlay-badge __size">{formatSize(video.size)}</div>
        <div className="__overlay-badge __duration">{formatDuration(video.durationInSeconds)}</div>
        <div className="__overlay-buttons-container">
          <div className="__overlay-buttons">
            <Link to={`videos/${video.videoId}`}>
              <button className="__overlay-button --small" aria-label="edit">
                <EditIcon fontSize="inherit" />
              </button>
            </Link>
            <button className="__overlay-button" aria-label="play" onClick={() => api.playVideo(video.videoId)}>
              <PlayArrowIcon fontSize="inherit" />
            </button>
            <button className="__overlay-button --small" aria-label="delete" onClick={() => onVideoToDeleteChange(video)}>
              <DeleteIcon fontSize="inherit" />
            </button>
          </div>
        </div>
      </div>
      <div className="__title" title={video.title}>{video.title}</div>
    </div>
  );
}

function formatResolution(width: number, height: number) {
  const resolution = `${width}x${height}`;

  switch (resolution) {
    case '3840x2160':
      return '4K';
    case '1920x1080':
      return '1080p';
    case '1280x720':
      return '720p'
    case '640x480':
      return '480p'
    default:
      return resolution;
  }
}
