// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

    $(document).ready(function() {
    $("a.like-button").click(function(event) {
        event.preventDefault();
        var postId = $(this).data("postid");
        $.ajax({
            url: '/Publication/CreateLike',
            type: 'POST',
            data: { postId: postId },
            success: function(response) {
                $("#like-count-" + postId).text(response.likeCount);
            },
            error: function(xhr, status, error) {
                console.error('Error creating like:', error);
            }
        });
    });
});

$(document).ready(function() {
    $("#subscribeButton").click(function(event) {
        event.preventDefault();
        var userId = $(this).data("userid");

        $.ajax({
            url: '/User/ToggleSubscription',
            type: 'POST',
            data: { userId: userId },
            success: function(response) {
                if (response.isSubscribed) {
                    $("#subscribeButton").text("Unsubscribe");
                } else {
                    $("#subscribeButton").text("Subscribe");
                }
                $("#followingCount").text(response.followingCount);
            },
            error: function(xhr, status, error) {
                console.error('Error toggling subscription:', error);
            }
        });
    });
});


$(".delete-button").click(function(event) {
    event.preventDefault();
    var postId = $(this).data("postid");
    $.ajax({
        url: '/User/DeletePost',
        type: 'POST',
        data: { postId: postId },
        success: function(response) {
            if (response.success) {
                $("#postCount").text(response.postCount);
                $("#post-" + postId).remove();
            } else {
                console.error('Ошибка при удалении поста:', response.error);
            }
        },
        error: function(xhr, status, error) {
            console.error('Ошибка при удалении поста:', error);
        }
    });
});

$(".save-edit-button").click(function(event) {
    event.preventDefault();
    var postId = $(this).data("postid");
    var newDescription = $(".edit-description[data-postid='" + postId + "']").val();
    $.ajax({
        url: '/User/EditPostDescription',
        type: 'POST',
        data: { postId: postId, description: newDescription },
        success: function(response) {
            if (response.success) {
                $("#post-description-" + postId).text(newDescription);
                $("#editPostModal-" + postId).modal('hide');
            } else {
                console.error('Ошибка при редактировании поста:', response.error);
            }
        },
        error: function(xhr, status, error) {
            console.error('Ошибка при редактировании поста:', error);
        }
    });
});