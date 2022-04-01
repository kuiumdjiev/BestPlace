﻿using BestPlace.Core.Contracts;
using BestPlace.Core.Models.Category;
using BestPlace.Infrastructure.Data;
using BestPlace.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BestPlace.Core.Services;

public class CategoryService:ICategoryService
{
    private readonly IApplicatioDbRepository repository;

    public CategoryService(IApplicatioDbRepository repository)
    {
        this.repository = repository;
    }

    public async Task<IEnumerable<CategoryListViewModel>> All()
    {
        var categorys = await this.repository.All<Category>()
            .Select(x => new CategoryListViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Source = x.Image
            }).ToListAsync();

        return categorys;
    }

    public async Task<bool> AddCategory(CategoryAddViewModel model)
    {

        byte[] bytes = null;
        using (MemoryStream ms = new MemoryStream())

        {

            model.Image.OpenReadStream().CopyTo(ms);

            bytes = ms.ToArray();

        }

        
        var category = new Category
        {
            Name = model.Name,
            Image = bytes,
        };
        try
        {
            await this.repository.AddAsync(category);
            await this.repository.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }

    }

    public async Task<CategoryEditViewModel> GetCategoryForEdit(Guid id)
    {
        var category = await this.repository.GetByIdAsync<Category>(id);

        return new CategoryEditViewModel
        {
            Id = category.Id,
            Name = category.Name,
        };

    }

    public async Task<bool> DeleteCategory(Guid id)
    {
        var category = await this.repository.GetByIdAsync<Category>(id);
        if (category == null)
        {
            return false;
        }

        try
        {
             this.repository.Delete(category);
            await this.repository.SaveChangesAsync();
            return true;
        }
        catch
        {

            return false;
        }
    }

    public async Task<bool> EditCategory(CategoryEditViewModel model)
    {
        var category = await this.repository.GetByIdAsync<Category>(model.Id);
        if (category == null)
        {
            return false;
        }
        byte[] bytes = null;
        using (MemoryStream ms = new MemoryStream())

        {

            model.Image.OpenReadStream().CopyTo(ms);

            bytes = ms.ToArray();

        }
        try
        {
            category.Name = model.Name;
            category.Image =bytes;
            await this.repository.SaveChangesAsync();
            return true;
        }
        catch
        {

            return false;
        }
    }
}