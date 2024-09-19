import { Auction, AuctionFinished } from '@/types'
import Image from 'next/image'
import Link from 'next/link'
import React from 'react'
import { numberWithCommas } from '../lib/numberWithComma'

type Props = {
    finishedAuction: AuctionFinished
    auction: Auction
}

export default function AuctionFinishedToast({finishedAuction, auction}: Props) {
  return (
    <Link href={`/auctions/details/${finishedAuction.auctionId}`} className='flex flex-col items-center'>
        <div className='flex flex-row items-center gap-2'>
            <Image 
                src={auction.imageUrl}
                alt='Image of car'
                height={80}
                width={80}
                className='rounded-lg w-auto h-auto'
            />
            <div className='flex flex-col'>
                <span>
                    Auction finished: {auction.make} {auction.model}
                </span>
                {finishedAuction.itemSold && finishedAuction.amount 
                    ? (<p>Winner: {finishedAuction.winner} for $${numberWithCommas(finishedAuction.amount)}</p>)
                    : (<p>Item did not sell</p>)
                }
            </div>
        </div>
    </Link>
  )
}
