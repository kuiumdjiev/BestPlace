﻿using System.ComponentModel;
using BestPlace.Core.Contracts;
using BestPlace.Core.Models.Item;
using BestPlace.Infrastructure.Data;
using BestPlace.Infrastructure.Data.Identity;
using BestPlace.Infrastructure.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BestPlace.Core.Services;

public class ItemService : IItemService
{
    private readonly IApplicatioDbRepository repository;

    public ItemService(IApplicatioDbRepository repository)
    {
        this.repository = repository;
    }

    public async Task<ItemQueryViewModel> AllPublic(Guid categoryId, string query)
    {
        var items = await this.repository.All<Item>()
            .Where(x => x.CategoryId == categoryId)
            .Select(x => new ItemPublicListViewModel()
            {
                Id = x.Id,
                Label = x.Label,
                Price = x.Price,
                Image = x.Images.Select(x => new ItemImageDetailsViewModel
                {
                    Id = x.Id,
                }).First()
            }).ToListAsync();

        if (query != null)
        {

            return new ItemQueryViewModel
            {
                CategoryId = categoryId,
                Query = query,
                Items = items.Where(x => x.Label.ToLower().Contains(query.ToLower()))
            };


        }

        return new ItemQueryViewModel
        {
            CategoryId = categoryId,
            Query = query,
            Items = items
        };
    }

    public async Task<IEnumerable<ItemListViewModel>> All()
    {
        var items = await this.repository.All<Item>()
            .Select(x => new ItemListViewModel
            {
                Id = x.Id,
                Label = x.Label,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                Price = x.Price,
                OwnerId = x.OwnerId,
                Owner = $"{x.Owner.FirstName} {x.Owner.LastName}"
            }).ToListAsync();
        return items;
    }



    public async Task IsMine(Guid id, string userId)
    {
        var user = await this.repository.GetByIdAsync<ApplicationUser>(userId);
        if (user == null) throw new ArgumentException("Unknow user");
        var item = user.MyItems.Any(x => x.Id == id);
        if (item == false) throw new ArgumentException("This is not your item");
    }

    public async Task<ItemEditViewModel> GetItemForEdit(Guid id, string userId)
    {
        var item = await this.repository.All<Item>().Include(x => x.Category).Include(x => x.Owner).Include(x => x.Images).FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) throw new ArgumentException("Unknow item");
        await IsMine(id, userId);

        var itemForEdit = new ItemEditViewModel()
        {
            Id = item.Id,
            Label = item.Label,
            Description = item.Description,
            CategoryId = item.CategoryId.ToString(),
            Price = item.Price,
        };

        itemForEdit.ViewImage = item.Images.Select(x => new ItemImageDetailsViewModel() { Id = x.Id }).ToList();
        return itemForEdit;
    }

    public async Task Edit(ItemEditViewModel model, string userId)
    {
        var item = await this.repository.GetByIdAsync<Item>(model.Id);
        if (item == null) throw new ArgumentException("Unknow item");
        await IsMine(model.Id, userId);
        foreach (var image in model.Images)
        {
            byte[] bytes = null;
            using (MemoryStream ms = new MemoryStream())
            {

                await image.OpenReadStream().CopyToAsync(ms);

                bytes = ms.ToArray();

            }

            var img = new Image()
            {
                Source = bytes
            };

            var itemImage = new ItemImages()

            {
                ItemId = item.Id,
                ImageId = img.Id
            };

            item.Images.Add(itemImage);
            await this.repository.AddAsync(itemImage);
            await this.repository.AddAsync(img);
        }



        item.Label = model.Label;
        item.Description = model.Description;
        item.Price = model.Price;
        item.CategoryId = Guid.Parse(model.CategoryId);
        await this.repository.SaveChangesAsync();
    }


    public async Task<ItemDetailsViewModel> GetItemDetails(Guid id)
    {
        var item = await this.repository.All<Item>().Include(x => x.Images).Include(x => x.Owner).Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) throw new ArgumentException("Unknown item");
       

        var itemDetails = new ItemDetailsViewModel
        {
            Id = item.Id,
            Label = item.Label,
            Description = item.Description,
            Price = item.Price,
            OwnerId = item.OwnerId,
            OwnerName = $"{item.Owner.FirstName} {item.Owner.LastName}",
            CategoryId = item.CategoryId,
            CategoryName = item.Category.Name,
            Images = item.Images.Select(y => new ItemImageDetailsViewModel
            {
                Id = y.Id,
            }).ToList()
        };
        return itemDetails;
    }

    public async Task<ItemDetailsViewModel> GetItemDetailsAsAdmin(Guid id)
    {
        var item = await this.repository.All<Item>().Include(x => x.Images).Include(x => x.Owner).Include(x => x.Category).FirstOrDefaultAsync(x => x.Id == id);
        if (item == null) throw new ArgumentException("Unknown item");

        var itemDetails = new ItemDetailsViewModel
        {
            Id = item.Id,
            Label = item.Label,
            Description = item.Description, 
            Price = item.Price,
            OwnerId = item.OwnerId,
            OwnerName = $"{item.Owner.FirstName} {item.Owner.LastName}",
            CategoryId = item.CategoryId,
            CategoryName = item.Category.Name,
            
            Images = item.Images.Select(y => new ItemImageDetailsViewModel
            {
                Id = y.Id,
            }).ToList()
        };
        return itemDetails;
    }

    public async Task<bool> AddItem(ItemAddViewModel model, string userId)
    {
        try
        {
            var item = new Item()
            {
                Label = model.Label,
                Description = model.Description,
                Price = model.Price,
                CategoryId = Guid.Parse(model.CategoryId),
                OwnerId = userId
            };

            var user = await this.repository.GetByIdAsync<ApplicationUser>(userId);
            var category = await this.repository.GetByIdAsync<Category>(Guid.Parse(model.CategoryId));

            user.MyItems.Add(item);
            category.Items.Add(item);

            item.Owner = user;
            item.Category = category;

            var images = new List<ItemImages>();
            foreach (var image in model.Images)
            {
                byte[] bytes = null;
                using (MemoryStream ms = new MemoryStream())
                {

                    await image.OpenReadStream().CopyToAsync(ms);

                    bytes = ms.ToArray();

                }

                var img = new Image
                {
                    Source = bytes
                };
                var itemImage = new ItemImages()

                {
                    ItemId = item.Id,
                    ImageId = img.Id
                };

                images.Add(itemImage);
                await this.repository.AddAsync(img);
                await this.repository.AddAsync(itemImage);

            }


            await this.repository.AddAsync(item);

            await this.repository.SaveChangesAsync();
            return true;
        }
        catch 
        {
            return false;
        }
       

    }



    public async Task DeleteItem(Guid id ,string userId)
    {
        var item = await this.repository.GetByIdAsync<Item>(id);
        if (item == null) throw new ArgumentException("Unknown item");
        IsMine( id,  userId);
        item.Images = new List<ItemImages>();
        await this.repository.SaveChangesAsync();

        this.repository.Delete<Item>(item);
        await this.repository.SaveChangesAsync();
    }

    public async Task DeleteItemAsAdmin(Guid id)
    {
        var item = await this.repository.All<Item>().Include(x=>x.Images).FirstOrDefaultAsync(x=>x.Id==id);
        if (item == null) throw new ArgumentException("Unknown item");
        item.Images = new List<ItemImages>();
        this.repository.Delete<Item>(item);
        await this.repository.SaveChangesAsync();
    }
}