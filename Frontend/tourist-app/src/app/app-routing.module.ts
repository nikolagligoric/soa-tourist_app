import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { HomeComponent } from './home/home.component';
import { RegisterComponent } from './register/register.component';
import { BlockUserComponent } from './block-user/block-user.component';
import { ProfileComponent } from './profile/profile.component';
import { UserProfileComponent } from './user-profile/user-profile.component';
import { ToursComponent } from './tours/tours.component';
import { CartComponent } from './cart/cart.component';
import { PurchasesComponent } from './purchases/purchases.component';
import { TourDetailsComponent } from './tour-details/tour-details.component';
import { ActiveTourComponent } from './active-tour/active-tour.component';
import { BlogComponent } from './blog/blog.component';
import { CurrentLocationComponent } from './current-location/current-location.component';

const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },

  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'admin/users', component: BlockUserComponent },
  { path: 'profile', component: ProfileComponent },
  { path: 'profile/:username', component: UserProfileComponent },
  { path: 'home', component: HomeComponent },
  { path: 'tours', component: ToursComponent },
  { path: 'cart', component: CartComponent },
  { path: 'purchases', component: PurchasesComponent },
  { path: 'tours/:id', component: TourDetailsComponent },
  { path: 'active-tour', component: ActiveTourComponent },
];  
  { path: 'blogs', component: BlogComponent },
  { path: 'current-location', component: CurrentLocationComponent },
  { path: 'home', component: HomeComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
