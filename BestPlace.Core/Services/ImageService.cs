﻿using BestPlace.Core.Contracts;
using BestPlace.Infrastructure.Data;
using BestPlace.Infrastructure.Data.Identity;
using BestPlace.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BestPlace.Core.Services;

public class ImageService : IImageService
{
    private readonly IApplicatioDbRepository repository;

    public ImageService(IApplicatioDbRepository repository)
    {
        this.repository = repository;
    }

    public async Task<byte[]> GetCategoryImage(Guid id)
    {
        var category = await this.repository.All<Category>().Include(x=>x.Image).FirstOrDefaultAsync(x=>x.Id==id);
        if (category == null) throw new ArgumentException("Unknown category");
        return category.Image.Source;
    }
    public async Task<byte[]> GetProfileImage(Guid id)
    {
        var image = await this.repository.All<Image>().FirstOrDefaultAsync(x => x.Id == id);
        return image.Source;
    }
    public async Task<byte[]> GetItemImage(Guid id)
    {
        var itemImage = await this.repository.All<ItemImages>().Include(x=>x.Item).Include(x=>x.Image).FirstOrDefaultAsync(x=>x.Id==id);
        if (itemImage == null) throw new ArgumentException("Unknown image");

        return itemImage.Image.Source;
    }

    public async Task DeleteItemImage(Guid id)
    {
        var itemImage = await this.repository.All<ItemImages>().Include(x => x.Item).Include(x => x.Image).FirstOrDefaultAsync(x => x.Id == id);
        if (itemImage == null) throw new ArgumentException("Unknown image");
        this.repository.Delete(itemImage);
        await this.repository.SaveChangesAsync();
    }

    public async  Task AddImageProfile(Image image, string userId)
    {
        var user = await this.repository.GetByIdAsync<ApplicationUser>(userId);
        if(user==null) throw new ArgumentException("Unknown user");
        await this.repository.AddAsync(image);
        await this.repository.SaveChangesAsync();
    }
}